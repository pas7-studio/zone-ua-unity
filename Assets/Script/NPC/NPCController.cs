using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Animator))]
public class NPCController : MonoBehaviour
{
    // Variables
    public GameObject weapon;
    private GameObject mainWeapon;

    private Health health;
    public bool isLogicTurnOn = true;
    public Transform[] patrolPoints;
    public string currentLogicState;
    public string npcName;
    public bool isHaveRootLocation;

    // Movement variables
    public float moveSpeed = 4f;
    public float minDelay = 3f;
    public float maxDelay = 7f;

    public float detectionRadius = 12f;
    public float shootRadius = 10f;

    // Private variables
    [SerializeField]
    private Transform patrolPointTarget;
    private int currentPointIndex = 1;

    public Transform targetNPC;
    public Transform targetObject;

    // Logic timing variables
    public float mediumTermLogicInterval = 1f; // Process once per second
    public float longTermLogicInterval = 5f; // Process every 5 seconds
    private float mediumTermLogicTimer = 0f;
    private float longTermLogicTimer = 0f;

    private bool isTargetRiched = true;

    private Rigidbody2D rgb2d;
    private WeaponController weaponController;

    public float weaponRotateSpeed = 7f;

    private Animator animator;

    private GlobalSystem globalSystem;

    // Methods
    void Start()
    {
        globalSystem = globalSystem = GameObject.FindGameObjectWithTag("System").GetComponent<GlobalSystem>();

        SpawnWeapon();
        SetWeaponShow(true);

        animator = GetComponent<Animator>();

        currentLogicState = "Patrol";
        rgb2d = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        weaponController = GetComponentInChildren<WeaponController>();

        if (weaponController != null)
        {
            weaponController.fireMode = weaponController.isBurstAvailible ? WeaponController.FireMode.Burst : WeaponController.FireMode.Single;
            weaponController.rotationSpeed = weaponRotateSpeed;
        }

        LongTermLogic();
    }

    void FixedUpdate()
    {
        if (isLogicTurnOn)
        {
            // Increment the logic timers
            mediumTermLogicTimer += Time.deltaTime;
            longTermLogicTimer += Time.deltaTime;

            ShortTermLogic();

            if (mediumTermLogicTimer >= mediumTermLogicInterval)
            {
                MediumTermLogic();
            }

            if (longTermLogicTimer >= longTermLogicInterval)
            {
                LongTermLogic();
            }
        }
    }

    void ShortTermLogic()
    {
        switch (currentLogicState)
        {
            case "Patrol":
                PatrolLogic();
                break;
        }

        if (targetNPC)
        {

            RotateToTarget(targetNPC);

            if (Vector2.Distance(transform.position, targetNPC.transform.position) < shootRadius)
            {
                if (weaponController)
                {
                    if (weaponController.currentAmmo > 0)
                    {
                        weaponController.FireWithModes();
                    }
                    else
                    {
                        ReloadWeapon();
                    }
                }
            }
        }
        else
        {
            if (weaponController.currentAmmo != weaponController.weapon.weaponAmmoMax)
            {
                ReloadWeapon();
            }
        }
    }

    void MediumTermLogic()
    {
        CheckNPCInRadius();
    }

    private bool isChooseNextPatrolPointStarted = false;
    void LongTermLogic()
    {
        //CHANGE GLOBAL currentLogicState here

        // if isHaveRootLocation -> Patrol

        //TODO MAKE MORE COMPLEX
        if (patrolPoints.Length > 0 && isTargetRiched && !isChooseNextPatrolPointStarted)
        {
            // Choose a random delay before moving to the next point
            float delay = UnityEngine.Random.Range(minDelay, maxDelay);
            Invoke("ChooseNextPatrolPoint", delay);
            isChooseNextPatrolPointStarted = true;
        }
    }

    private float currentSpeed = 0f;
    void PatrolLogic()
    {
        if (patrolPoints.Length > 0 && patrolPointTarget && !isTargetRiched)
        {
            currentSpeed = moveSpeed;
            rgb2d.MovePosition(Vector2.MoveTowards(transform.position, patrolPointTarget.position, moveSpeed * Time.deltaTime));

            //Rotate npc to patrolPoint if no enemy here
            if (targetNPC == null)
            {
                RotateToTarget(targetObject);
            }

            // Check if we've reached the current patrol point
            if (transform.position == patrolPointTarget.position)
            {
                currentSpeed = 0f;
                isTargetRiched = true;
            }

            // Set animation of moving
            animator.SetFloat("Speed", currentSpeed);
        }
    }

    void RotateToTarget(Transform target)
    {
        Vector3 direction = target.transform.position - transform.position;

        if (direction.x > 0 && transform.rotation.y == 1f)
        {
            transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        }
        else if (direction.x <= 0 && transform.rotation.y == 0f)
        {
            transform.rotation = new Quaternion(0f, 180f, 0f, 0f);
        }
    }

    void ChooseNextPatrolPoint()
    {
        // Increment the current point index, wrapping around to the start if necessary
        isTargetRiched = false;
        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        patrolPointTarget = patrolPoints[currentPointIndex];

        SetTarget(patrolPointTarget, TargetType.OBJECT); // make SetTarget(null, TargetType.OBJECT) somewhere
        isChooseNextPatrolPointStarted = false;
    }

    void PlayerMoveToLogic()
    {
        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            var pos = Vector2.MoveTowards(transform.position, player.transform.position, moveSpeed * Time.deltaTime);
            rgb2d.MovePosition(pos);
        }
    }

    void CheckNPCInRadius()
    {
        // Check for targets in detection radius
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        targets = Array.FindAll(targets, c => c.gameObject != gameObject && c.gameObject.GetComponent<Health>() && c.gameObject.GetComponent<Health>().getIsAlive());

        if (targets.Length == 0)
        {
            SetTarget(null, TargetType.NPC);
        }

        foreach (Collider2D targetCollider in targets)
        {
            // Check if the target is the player or another NPC
            if ((targetCollider.CompareTag("Player") || targetCollider.CompareTag("Enemy")))
            {
                // Set the target to the detected object
                SetTarget(targetCollider.transform, TargetType.NPC);
            }
        }
    }

    public enum TargetType {
        NPC,
        OBJECT,
        BOTH
    }
    public void SetTarget(Transform newTarget, TargetType targetType)
    {
        if(targetType == TargetType.NPC) {
            targetNPC = newTarget;
            if (weaponController)
            {
                weaponController.SetNPCTarget(newTarget);
            }
        } else if (targetType == TargetType.OBJECT) {
            targetObject = newTarget;
            if (weaponController)
            {
                weaponController.SetObjectTarget(newTarget);
            }
        }
        else
        {
            targetNPC = newTarget;
            targetObject = newTarget;
            if (weaponController)
            {
                weaponController.SetNPCTarget(newTarget);
                weaponController.SetObjectTarget(newTarget);
            }
        }
    }

    private void ReloadWeapon()
    {
        weaponController.Reload();
    }

    public void SetWeaponShow(bool state)
    {
        mainWeapon.SetActive(state);
    }

    private void SpawnWeapon()
    {
        mainWeapon = Instantiate(weapon, new Vector3(transform.position.x - globalSystem.weaponXOffset * 10, transform.position.y - globalSystem.weaponYOffset * 10, transform.position.z), transform.rotation, transform);
        var anim = mainWeapon.GetComponent<Animator>();
        if(anim != null)
        {
            anim.applyRootMotion = true;
        }
    }

    public void StopAllWeaponCoroutines()
    {
        weaponController.StopAllCoroutines();
    }
}