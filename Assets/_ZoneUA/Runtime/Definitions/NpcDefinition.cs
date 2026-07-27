using UnityEngine;

namespace ZoneUA.AI
{
    [CreateAssetMenu(fileName = "NPC", menuName = "Zone UA/AI/NPC Definition")]
    public sealed class NpcDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField, Tooltip("Designer-facing NPC archetype name.")]
        private string displayName = "NPC";

        [Header("Movement")]
        [SerializeField, Min(0f)] private float movementSpeed = 2f;
        [SerializeField, Min(0f)] private float acceleration = 12f;
        [SerializeField, Min(0f)] private float stoppingDistance = 0.75f;

        [Header("Perception")]
        [SerializeField, Min(0f)] private float detectionRadius = 8f;
        [SerializeField, Range(0f, 360f)] private float fieldOfView = 180f;
        [SerializeField, Min(0f)] private float loseTargetDelay = 3f;
        [SerializeField, Min(0.02f), Tooltip("Seconds between target-sensor decisions.")]
        private float decisionInterval = 0.25f;

        [Header("Combat")]
        [SerializeField, Min(0f)] private float preferredAttackDistance = 5f;
        [SerializeField, Min(0f)] private float fleeHealthFraction = 0.15f;
        [SerializeField, Min(0f)] private float aimRotationSpeed = 10f;

        [Header("Patrol")]
        [SerializeField, Min(0f)] private float patrolRadius = 5f;
        [SerializeField, Min(0f)] private float minimumPatrolWait = 1f;
        [SerializeField, Min(0f)] private float maximumPatrolWait = 4f;

        public string DisplayName => displayName;
        public float MovementSpeed => movementSpeed;
        public float Acceleration => acceleration;
        public float StoppingDistance => stoppingDistance;
        public float DetectionRadius => detectionRadius;
        public float FieldOfView => fieldOfView;
        public float LoseTargetDelay => loseTargetDelay;
        public float DecisionInterval => decisionInterval;
        public float PreferredAttackDistance => preferredAttackDistance;
        public float FleeHealthFraction => fleeHealthFraction;
        public float AimRotationSpeed => aimRotationSpeed;
        public float PatrolRadius => patrolRadius;
        public float MinimumPatrolWait => minimumPatrolWait;
        public float MaximumPatrolWait => maximumPatrolWait;

        private void OnValidate()
        {
            displayName = string.IsNullOrWhiteSpace(displayName) ? name : displayName.Trim();
            movementSpeed = Mathf.Max(0f, movementSpeed);
            acceleration = Mathf.Max(0f, acceleration);
            stoppingDistance = Mathf.Max(0f, stoppingDistance);
            detectionRadius = Mathf.Max(0f, detectionRadius);
            loseTargetDelay = Mathf.Max(0f, loseTargetDelay);
            decisionInterval = Mathf.Max(0.02f, decisionInterval);
            preferredAttackDistance = Mathf.Max(stoppingDistance, preferredAttackDistance);
            fleeHealthFraction = Mathf.Clamp01(fleeHealthFraction);
            patrolRadius = Mathf.Max(0f, patrolRadius);
            minimumPatrolWait = Mathf.Max(0f, minimumPatrolWait);
            maximumPatrolWait = Mathf.Max(minimumPatrolWait, maximumPatrolWait);
        }
    }
}
