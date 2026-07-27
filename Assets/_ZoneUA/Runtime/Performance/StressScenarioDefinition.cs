using UnityEngine;

namespace ZoneUA.Performance
{
    [CreateAssetMenu(fileName = "StressScenario", menuName = "Zone UA/Performance/Stress Scenario")]
    public sealed class StressScenarioDefinition : ScriptableObject
    {
        [SerializeField, Min(1f)] private float warmupSeconds = 5f;
        [SerializeField, Min(1f)] private float captureSeconds = 30f;
        [SerializeField, Min(0)] private int requestedNpcCount = 50;
        [SerializeField, Min(0)] private int requestedProjectileBurstsPerSecond = 20;
        [SerializeField, Min(0)] private int requestedGeneratedObjectCount = 3000;
        [SerializeField] private bool regenerateWorldBeforeCapture = true;
        [SerializeField] private PerformanceBudgetProfile budgetProfile;

        public float WarmupSeconds => warmupSeconds;
        public float CaptureSeconds => captureSeconds;
        public int RequestedNpcCount => requestedNpcCount;
        public int RequestedProjectileBurstsPerSecond => requestedProjectileBurstsPerSecond;
        public int RequestedGeneratedObjectCount => requestedGeneratedObjectCount;
        public bool RegenerateWorldBeforeCapture => regenerateWorldBeforeCapture;
        public PerformanceBudgetProfile BudgetProfile => budgetProfile;
    }
}
