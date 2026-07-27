using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Animator))]
public sealed class NPCController : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private const int DetectionBufferSize = 32;

    [Header("Identity")]
    [SerializeField] private string npcName;
    [SerializeField] private bool isHaveRootLocation;
    [SerializeField] private string currentLogicState = "Patrol";

    [Header("Equipment")]
    [SerializeField] private GameObject weapon;
    [SerializeField, Min(0f)] private float weaponRotateSpeed = 7f;

    [Header("Logic")]
    [SerializeField] private bool isLogicTurnOn = true;
    [SerializeField, Min(0.02f)] private float mediumTermLogicInterval = 1f;
    [SerializeField, Min(0.02f)] private float longTermLogicInterval = 5f;
    [SerializeField] private LayerMask detectionLayers = ~0;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField, Min(0f)] private float moveSpeed = 4f;
    [SerializeField, Min(0f)] private float minDelay = 3f;
    [SerializeField, Min(0f)] private float maxDelay = 7f;

    [Header("Combat")]
    [SerializeField, Min(0f)] private float detectionRadius = 12f;
    [SerializeField, Min(0f)] private float shootRadius = 10f;

    [Header("Runtime Debug")]
    [SerializeField] private Transform patrolPointTarget;
    [SerializeField] private Transform targetNPC;
    [SerializeField] private Transform targetObject;
    [SerializeField] private float currentSpeed;

    private readonly Collider2D[] detectionBuffer = new Collider2D[DetectionBufferSize];

    private Rigidbody2D body;
    private Animator animator;
    private Health health;
    private WeaponController weaponController;
    private GameObject mainWeapon;

    private int currentPointIndex = -1;
    private float mediumTermLogicTimer;
    private float longTermLogicTimer;
    private float nextPatrolDecisionTime;
    private bool patrolDecisionScheduled;
    private bool isTargetReached = true;

    private enum LogicState
    {
        Patrol
    }

    private LogicState logicState = LogicState.Patrol;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
    }

    private void Start()
    {
        SpawnWeapon();
        SetWeaponShow(true);

        weaponController = GetComponentInChildren<WeaponController>();
        if (weaponController != null)
        {
            weaponController.CurrentFireMode = weaponController.SupportsBurst
                ? WeaponController.FireMode.Burst
                : WeaponController.FireMode.Single;
            weaponController.RotationSpeed = weaponRotateSpeed;
        }

        currentLogicState = logicState.ToString();
        ScheduleNextPatrolDecision();
    }

    private void FixedUpdate()
    {
        if (!isLogicTurnOn || !health.IsAlive)
        {
            return;
        }

        float deltaTime = Time.fixedDeltaTime;
        mediumTermLogicTimer += deltaTime;
        longTermLogicTimer += deltaTime;

        ShortTermLogic();
        UpdatePatrolDecision();

        if (mediumTermLogicTimer >= mediumTermLogicInterval)
        {
            mediumTermLogicTimer = 0f;
            MediumTermLogic();
        }

        if (longTermLogicTimer >= longTermLogicInterval)
        {
            longTermLogicTimer = 0f;
            LongTermLogic();
        }
    }

    private void ShortTermLogic()
    {
        if (logicState == LogicState.Patrol)
        {
            PatrolLogic();
        }

        if (targetNPC != null)
        {
            RotateToTarget(targetNPC);

            float squaredDistance = ((Vector2)targetNPC.position - body.position).sqrMagnitude;
            if (squaredDistance <= shootRadius * shootRadius && weaponController != null)
            {
                if (weaponController.CurrentAmmo > 0)
                {
                    weaponController.FireWithModes();
                }
                else
                {
                    weaponController.Reload();
                }
            }
        }
        else if (weaponController != null &&
                 weaponController.CurrentAmmo < weaponController.WeaponData.MaximumAmmo)
        {
            weaponController.Reload();
        }
    }

    private void MediumTermLogic()
    {
        CheckNPCInRadius();
    }

    private void LongTermLogic()
    {
        if (isTargetReached && !patrolDecisionScheduled)
        {
            ScheduleNextPatrolDecision();
        }
    }

    private void ScheduleNextPatrolDecision()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            patrolDecisionScheduled = false;
            return;
        }

        nextPatrolDecisionTime = Time.time + Random.Range(minDelay, maxDelay);
        patrolDecisionScheduled = true;
    }

    private void UpdatePatrolDecision()
    {
        if (!isTargetReached ||
            !patrolDecisionScheduled ||
            Time.time < nextPatrolDecisionTime)
        {
            return;
        }

        patrolDecisionScheduled = false;
        ChooseNextPatrolPoint();
    }

    private void PatrolLogic()
    {
        if (patrolPointTarget == null || isTargetReached)
        {
            currentSpeed = 0f;
            animator.SetFloat(SpeedHash, currentSpeed);
            return;
        }

        currentSpeed = moveSpeed;
        Vector2 targetPosition = patrolPointTarget.position;
        Vector2 nextPosition = Vector2.MoveTowards(
            body.position,
            targetPosition,
            moveSpeed * Time.fixedDeltaTime);

        body.MovePosition(nextPosition);

        if (targetNPC == null)
        {
            RotateToTarget(targetObject);
        }

        if ((targetPosition - nextPosition).sqrMagnitude <= 0.0001f)
        {
            currentSpeed = 0f;
            isTargetReached = true;
            ScheduleNextPatrolDecision();
        }

        animator.SetFloat(SpeedHash, currentSpeed);
    }

    private void RotateToTarget(Transform target)
    {
        if (target == null)
        {
            return;
        }

        bool shouldFaceLeft = target.position.x <= transform.position.x;
        float targetYRotation = shouldFaceLeft ? 180f : 0f;

        if (!Mathf.Approximately(transform.eulerAngles.y, targetYRotation))
        {
            transform.rotation = Quaternion.Euler(0f, targetYRotation, 0f);
        }
    }

    private void ChooseNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            return;
        }

        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        patrolPointTarget = patrolPoints[currentPointIndex];
        isTargetReached = patrolPointTarget == null;
        if (isTargetReached)
        {
            ScheduleNextPatrolDecision();
        }

        SetTarget(patrolPointTarget, TargetType.OBJECT);
    }

    private void CheckNPCInRadius()
    {
        int hitCount = Physics2D.OverlapCircleNonAlloc(
            transform.position,
            detectionRadius,
            detectionBuffer,
            detectionLayers);

        Transform closestTarget = null;
        float closestSquaredDistance = float.PositiveInfinity;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D targetCollider = detectionBuffer[i];
            detectionBuffer[i] = null;

            if (targetCollider == null || targetCollider.transform.root == transform.root)
            {
                continue;
            }

            if (!targetCollider.CompareTag("Player") && !targetCollider.CompareTag("Enemy"))
            {
                continue;
            }

            Health targetHealth = targetCollider.GetComponentInParent<Health>();
            if (targetHealth == null || !targetHealth.IsAlive)
            {
                continue;
            }

            float squaredDistance =
                ((Vector2)targetCollider.transform.position - body.position).sqrMagnitude;

            if (squaredDistance >= closestSquaredDistance)
            {
                continue;
            }

            closestSquaredDistance = squaredDistance;
            closestTarget = targetHealth.transform;
        }

        SetTarget(closestTarget, TargetType.NPC);
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
            targetNPC = newTarget;
            weaponController?.SetNPCTarget(newTarget);
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

        Vector3 spawnPosition = transform.position;
        spawnPosition.x -= offset.x;
        spawnPosition.y -= offset.y;

        mainWeapon = Instantiate(weapon, spawnPosition, transform.rotation, transform);

        if (mainWeapon.TryGetComponent(out Animator weaponAnimator))
        {
            weaponAnimator.applyRootMotion = true;
        }
    }

    public void StopAllWeaponCoroutines()
    {
        weaponController?.StopAllCoroutines();
    }

    public void PrepareForDeath()
    {
        SetTarget(null, TargetType.BOTH);
        SetWeaponShow(false);
        StopAllWeaponCoroutines();
        isLogicTurnOn = false;
        currentSpeed = 0f;
        animator.SetFloat(SpeedHash, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    private void OnValidate()
    {
        mediumTermLogicInterval = Mathf.Max(0.02f, mediumTermLogicInterval);
        longTermLogicInterval = Mathf.Max(0.02f, longTermLogicInterval);
        maxDelay = Mathf.Max(minDelay, maxDelay);
        detectionRadius = Mathf.Max(0f, detectionRadius);
        shootRadius = Mathf.Clamp(shootRadius, 0f, detectionRadius);
    }
}
