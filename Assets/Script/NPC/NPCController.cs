using System;
using UnityEngine;
using ZoneUA.AI;
using ZoneUA.Factions;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
public sealed class NPCController : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private const int DetectionBufferSize = 32;

    [Header("Definition")]
    [SerializeField] private NpcDefinition definition;

    [Header("Identity")]
    [SerializeField] private string npcName;
    [SerializeField] private bool isHaveRootLocation;

    [Header("Equipment")]
    [SerializeField] private GameObject weapon;
    [SerializeField, Min(0f)] private float weaponRotateSpeed = 7f;

    [Header("Logic")]
    [SerializeField] private bool isLogicTurnOn = true;
    [SerializeField, Min(0.02f)] private float decisionInterval = 0.25f;
    [SerializeField] private LayerMask detectionLayers = ~0;
    [SerializeField] private LayerMask lineOfSightBlockingLayers;
    [SerializeField] private bool requireLineOfSight;

    [Header("Patrol Fallback")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField, Min(0f)] private float moveSpeed = 4f;
    [SerializeField, Min(0f)] private float stoppingDistance = 0.25f;
    [SerializeField, Min(0f)] private float minDelay = 3f;
    [SerializeField, Min(0f)] private float maxDelay = 7f;

    [Header("Combat Fallback")]
    [SerializeField, Min(0f)] private float detectionRadius = 12f;
    [SerializeField, Min(0f)] private float shootRadius = 10f;
    [SerializeField, Min(0f)] private float loseTargetDelay = 3f;
    [SerializeField, Range(0f, 1f)] private float fleeHealthFraction = 0.15f;

    [Header("Runtime Debug")]
    [SerializeField] private NpcState currentState;
    [SerializeField] private Transform patrolPointTarget;
    [SerializeField] private Transform targetNPC;
    [SerializeField] private Transform targetObject;
    [SerializeField] private float currentSpeed;

    private readonly Collider2D[] detectionBuffer = new Collider2D[DetectionBufferSize];
    private readonly NpcBrainState brain = new NpcBrainState();

    private Rigidbody2D body;
    private Animator animator;
    private Health health;
    private FactionMember factionMember;
    private WeaponController weaponController;
    private GameObject mainWeapon;
    private int currentPointIndex = -1;
    private float nextDecisionTime;
    private float nextPatrolDecisionTime;
    private bool patrolDecisionScheduled;

    public event Action<NpcState, NpcState> StateChanged;
    public event Action<Transform> TargetChanged;

    public NpcState CurrentState => brain.Current;
    public Transform CurrentTarget => targetNPC;
    public bool HasTarget => targetNPC != null;

    private float MovementSpeed => definition != null ? definition.MovementSpeed : moveSpeed;
    private float StopDistance => definition != null ? definition.StoppingDistance : stoppingDistance;
    private float DetectionRadius => definition != null ? definition.DetectionRadius : detectionRadius;
    private float AttackDistance => definition != null ? definition.PreferredAttackDistance : shootRadius;
    private float LoseTargetDelay => definition != null ? definition.LoseTargetDelay : loseTargetDelay;
    private float FleeThreshold => definition != null ? definition.FleeHealthFraction : fleeHealthFraction;
    private float DecisionInterval => definition != null ? definition.DecisionInterval : decisionInterval;
    private float MinimumPatrolWait => definition != null ? definition.MinimumPatrolWait : minDelay;
    private float MaximumPatrolWait => definition != null ? definition.MaximumPatrolWait : maxDelay;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        factionMember = GetComponent<FactionMember>();
        brain.Changed += HandleStateChanged;
    }

    private void Start()
    {
        SpawnWeapon();
        SetWeaponShow(true);
        weaponController = GetComponentInChildren<WeaponController>(true);
        if (weaponController != null)
        {
            weaponController.CurrentFireMode = weaponController.SupportsBurst
                ? WeaponController.FireMode.Burst
                : WeaponController.FireMode.Single;
            weaponController.RotationSpeed = definition != null
                ? definition.AimRotationSpeed
                : weaponRotateSpeed;
        }

        ChooseNextPatrolPoint();
        nextDecisionTime = Time.time;
        EvaluateState(Time.time);
    }

    private void OnDestroy()
    {
        brain.Changed -= HandleStateChanged;
    }

    private void FixedUpdate()
    {
        if (!isLogicTurnOn || !health.IsAlive)
        {
            EnterDeadState();
            return;
        }

        float now = Time.time;
        if (now >= nextDecisionTime)
        {
            nextDecisionTime = now + DecisionInterval;
            AcquireTarget();
            EvaluateState(now);
        }

        TickCurrentState(now);
        animator.SetFloat(SpeedHash, currentSpeed);
    }

    private void EvaluateState(float now)
    {
        float healthFraction = health.MaximumHealth > 0
            ? health.CurrentHealth / (float)health.MaximumHealth
            : 0f;
        float targetDistance = targetNPC != null
            ? Vector2.Distance(body.position, targetNPC.position)
            : float.PositiveInfinity;

        brain.SetTargetVisible(targetNPC != null, now);
        currentState = brain.Evaluate(
            health.IsAlive,
            healthFraction,
            FleeThreshold,
            patrolPoints != null && patrolPoints.Length > 0,
            targetDistance,
            AttackDistance,
            LoseTargetDelay,
            now);
    }

    private void TickCurrentState(float now)
    {
        switch (brain.Current)
        {
            case NpcState.Idle:
                StopMovement();
                TrySchedulePatrol(now);
                break;
            case NpcState.Patrol:
                TickPatrol(now);
                break;
            case NpcState.Chase:
                TickChase();
                break;
            case NpcState.Attack:
                TickAttack();
                break;
            case NpcState.Flee:
                TickFlee();
                break;
            case NpcState.Dead:
                StopMovement();
                break;
        }
    }

    private void TickPatrol(float now)
    {
        if (patrolPointTarget == null)
        {
            StopMovement();
            TrySchedulePatrol(now);
            return;
        }

        if (MoveTowards(patrolPointTarget.position, StopDistance))
        {
            patrolPointTarget = null;
            patrolDecisionScheduled = false;
            TrySchedulePatrol(now);
        }
        else
        {
            targetObject = patrolPointTarget;
            weaponController?.SetObjectTarget(targetObject);
        }
    }

    private void TickChase()
    {
        if (targetNPC == null)
        {
            StopMovement();
            return;
        }

        RotateToTarget(targetNPC);
        MoveTowards(targetNPC.position, AttackDistance * 0.9f);
    }

    private void TickAttack()
    {
        StopMovement();
        if (targetNPC == null || weaponController == null)
        {
            return;
        }

        RotateToTarget(targetNPC);
        weaponController.SetNPCTarget(targetNPC);
        if (weaponController.CurrentAmmo > 0)
        {
            weaponController.FireWithModes();
        }
        else
        {
            weaponController.Reload();
        }
    }

    private void TickFlee()
    {
        if (targetNPC == null)
        {
            StopMovement();
            return;
        }

        Vector2 away = body.position - (Vector2)targetNPC.position;
        if (away.sqrMagnitude <= 0.0001f)
        {
            away = Vector2.right;
        }

        Vector2 destination = body.position + away.normalized * Mathf.Max(AttackDistance, 1f);
        MoveTowards(destination, 0f);
    }

    private bool MoveTowards(Vector2 destination, float stopDistance)
    {
        Vector2 offset = destination - body.position;
        if (offset.sqrMagnitude <= stopDistance * stopDistance)
        {
            StopMovement();
            return true;
        }

        currentSpeed = MovementSpeed;
        Vector2 nextPosition = Vector2.MoveTowards(
            body.position,
            destination,
            MovementSpeed * Time.fixedDeltaTime);
        body.MovePosition(nextPosition);
        RotateToPosition(destination);
        return false;
    }

    private void StopMovement()
    {
        currentSpeed = 0f;
        body.linearVelocity = Vector2.zero;
    }

    private void AcquireTarget()
    {
        int hitCount = Physics2D.OverlapCircleNonAlloc(
            transform.position,
            DetectionRadius,
            detectionBuffer,
            detectionLayers);

        Transform bestTarget = null;
        float bestScore = float.NegativeInfinity;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D candidateCollider = detectionBuffer[i];
            detectionBuffer[i] = null;
            if (candidateCollider == null || candidateCollider.transform.root == transform.root)
            {
                continue;
            }

            Health candidateHealth = candidateCollider.GetComponentInParent<Health>();
            if (candidateHealth == null || !candidateHealth.IsAlive)
            {
                continue;
            }

            FactionMember candidateFaction = candidateHealth.GetComponent<FactionMember>();
            bool hostile = factionMember == null || factionMember.CanDamage(candidateFaction);
            bool visible = !requireLineOfSight || HasLineOfSight(candidateHealth.transform);
            float squaredDistance = ((Vector2)candidateHealth.transform.position - body.position).sqrMagnitude;
            float score = NpcTargetScoring.Score(squaredDistance, hostile, candidateHealth.IsAlive, visible);
            if (!NpcTargetScoring.IsBetter(score, bestScore))
            {
                continue;
            }

            bestScore = score;
            bestTarget = candidateHealth.transform;
        }

        SetTarget(bestTarget, TargetType.NPC);
    }

    private bool HasLineOfSight(Transform target)
    {
        if (target == null || lineOfSightBlockingLayers.value == 0)
        {
            return true;
        }

        RaycastHit2D hit = Physics2D.Linecast(body.position, target.position, lineOfSightBlockingLayers);
        return hit.collider == null || hit.collider.transform.root == target.root;
    }

    private void TrySchedulePatrol(float now)
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            return;
        }

        if (!patrolDecisionScheduled)
        {
            nextPatrolDecisionTime = now + UnityEngine.Random.Range(MinimumPatrolWait, MaximumPatrolWait);
            patrolDecisionScheduled = true;
        }

        if (now >= nextPatrolDecisionTime)
        {
            patrolDecisionScheduled = false;
            ChooseNextPatrolPoint();
        }
    }

    private void ChooseNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            patrolPointTarget = null;
            return;
        }

        for (int attempt = 0; attempt < patrolPoints.Length; attempt++)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
            if (patrolPoints[currentPointIndex] != null)
            {
                patrolPointTarget = patrolPoints[currentPointIndex];
                SetTarget(patrolPointTarget, TargetType.OBJECT);
                return;
            }
        }

        patrolPointTarget = null;
    }

    private void HandleStateChanged(NpcState previous, NpcState next)
    {
        currentState = next;
        if (next != NpcState.Attack)
        {
            weaponController?.StopFire();
        }
        StateChanged?.Invoke(previous, next);
    }

    private void RotateToTarget(Transform target)
    {
        if (target != null)
        {
            RotateToPosition(target.position);
        }
    }

    private void RotateToPosition(Vector2 targetPosition)
    {
        bool shouldFaceLeft = targetPosition.x <= transform.position.x;
        transform.rotation = Quaternion.Euler(0f, shouldFaceLeft ? 180f : 0f, 0f);
    }

    public enum TargetType
    {
        NPC,
        OBJECT,
        BOTH
    }

    public void SetTarget(Transform newTarget, TargetType targetType)
    {
        if (targetType == TargetType.NPC || targetType == TargetType.BOTH)
        {
            if (targetNPC != newTarget)
            {
                targetNPC = newTarget;
                TargetChanged?.Invoke(newTarget);
            }
            weaponController?.SetNPCTarget(newTarget);
            if (newTarget == null)
            {
                brain.ClearTarget();
            }
        }

        if (targetType == TargetType.OBJECT || targetType == TargetType.BOTH)
        {
            targetObject = newTarget;
            weaponController?.SetObjectTarget(newTarget);
        }
    }

    public void SetWeaponShow(bool state)
    {
        if (mainWeapon != null)
        {
            mainWeapon.SetActive(state);
        }
    }

    private void SpawnWeapon()
    {
        if (weapon == null)
        {
            return;
        }

        Vector2 offset = GlobalSystem.Instance != null
            ? GlobalSystem.Instance.WeaponSpawnOffset
            : Vector2.zero;
        Vector3 spawnPosition = transform.position - (Vector3)offset;
        mainWeapon = Instantiate(weapon, spawnPosition, transform.rotation, transform);
        if (mainWeapon.TryGetComponent(out Animator weaponAnimator))
        {
            weaponAnimator.applyRootMotion = true;
        }
    }

    public void StopAllWeaponCoroutines()
    {
        weaponController?.StopFire();
    }

    public void PrepareForDeath()
    {
        SetTarget(null, TargetType.BOTH);
        SetWeaponShow(false);
        weaponController?.StopFire();
        isLogicTurnOn = false;
        EnterDeadState();
    }

    private void EnterDeadState()
    {
        brain.Transition(NpcState.Dead);
        StopMovement();
        animator.SetFloat(SpeedHash, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, DetectionRadius);
    }

    private void OnValidate()
    {
        decisionInterval = Mathf.Max(0.02f, decisionInterval);
        maxDelay = Mathf.Max(minDelay, maxDelay);
        detectionRadius = Mathf.Max(0f, detectionRadius);
        shootRadius = Mathf.Clamp(shootRadius, 0f, detectionRadius);
        stoppingDistance = Mathf.Max(0f, stoppingDistance);
        loseTargetDelay = Mathf.Max(0f, loseTargetDelay);
        fleeHealthFraction = Mathf.Clamp01(fleeHealthFraction);
    }
}
