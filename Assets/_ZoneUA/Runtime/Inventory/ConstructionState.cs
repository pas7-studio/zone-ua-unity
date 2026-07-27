using System;

namespace ZoneUA.Inventory
{
    [Serializable]
    public sealed class ConstructionState
    {
        private float requiredWork;
        private float appliedWork;
        private bool resourcesCommitted;

        public ConstructionState(float requiredWork, float appliedWork = 0f, bool resourcesCommitted = false)
        {
            this.requiredWork = Math.Max(0.01f, requiredWork);
            this.appliedWork = Math.Clamp(appliedWork, 0f, this.requiredWork);
            this.resourcesCommitted = resourcesCommitted;
        }

        public float RequiredWork => requiredWork;
        public float AppliedWork => appliedWork;
        public bool ResourcesCommitted => resourcesCommitted;
        public bool IsComplete => appliedWork >= requiredWork;
        public float Progress01 => requiredWork <= 0f ? 1f : appliedWork / requiredWork;

        public bool CommitResources()
        {
            if (resourcesCommitted) return false;
            resourcesCommitted = true;
            return true;
        }

        public float ApplyWork(float amount)
        {
            if (!resourcesCommitted || IsComplete || amount <= 0f) return 0f;
            float previous = appliedWork;
            appliedWork = Math.Min(requiredWork, appliedWork + amount);
            return appliedWork - previous;
        }

        public void Restore(float newRequiredWork, float newAppliedWork, bool committed)
        {
            requiredWork = Math.Max(0.01f, newRequiredWork);
            appliedWork = Math.Clamp(newAppliedWork, 0f, requiredWork);
            resourcesCommitted = committed;
        }
    }
}
