using System;
using System.Collections.Generic;
using System.Linq;
using ZoneUA.Inventory;

namespace ZoneUA.Economy
{
    [Serializable]
    public sealed class HarvestState
    {
        public int remainingUnits;
        public float accumulatedWork;
        public float respawnRemaining;

        public bool IsDepleted => remainingUnits <= 0;

        public HarvestState(int totalUnits) => remainingUnits = Math.Max(0, totalUnits);

        public int ApplyWork(float work, float requiredWork, int unitsPerHarvest)
        {
            if (IsDepleted || work <= 0f) return 0;
            accumulatedWork += work;
            int harvested = 0;
            float threshold = Math.Max(0.01f, requiredWork);
            int batch = Math.Max(1, unitsPerHarvest);
            while (accumulatedWork >= threshold && remainingUnits > 0)
            {
                accumulatedWork -= threshold;
                int amount = Math.Min(batch, remainingUnits);
                remainingUnits -= amount;
                harvested += amount;
            }
            return harvested;
        }

        public void Restore(int remaining, float work, float respawn)
        {
            remainingUnits = Math.Max(0, remaining);
            accumulatedWork = Math.Max(0f, work);
            respawnRemaining = Math.Max(0f, respawn);
        }
    }

    [Serializable]
    public sealed class ProductionQueueEntry
    {
        public string recipeId = string.Empty;
        public int remainingCycles = 1;
        public float elapsed;
    }

    [Serializable]
    public sealed class ProductionQueueState
    {
        public List<ProductionQueueEntry> entries = new List<ProductionQueueEntry>();

        public void Enqueue(string recipeId, int cycles)
        {
            if (string.IsNullOrWhiteSpace(recipeId) || cycles <= 0) return;
            entries.Add(new ProductionQueueEntry { recipeId = recipeId.Trim(), remainingCycles = cycles });
        }

        public bool TryAdvance(float deltaTime, float duration, out string completedRecipeId)
        {
            completedRecipeId = string.Empty;
            if (entries.Count == 0 || deltaTime <= 0f) return false;
            ProductionQueueEntry current = entries[0];
            current.elapsed += deltaTime;
            if (current.elapsed < Math.Max(0.01f, duration)) return false;
            current.elapsed = 0f;
            current.remainingCycles--;
            completedRecipeId = current.recipeId;
            if (current.remainingCycles <= 0) entries.RemoveAt(0);
            return true;
        }

        public void Normalize()
        {
            entries = entries
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.recipeId) && x.remainingCycles > 0)
                .Select(x => new ProductionQueueEntry
                {
                    recipeId = x.recipeId.Trim(),
                    remainingCycles = x.remainingCycles,
                    elapsed = Math.Max(0f, x.elapsed)
                })
                .ToList();
        }
    }

    public enum WorkerJobKind { None, Harvest, Deliver, Produce }

    [Serializable]
    public sealed class WorkerJobState
    {
        public WorkerJobKind kind;
        public string targetObjectId = string.Empty;
        public bool resourcesReserved;

        public void Assign(WorkerJobKind jobKind, string targetId)
        {
            kind = jobKind;
            targetObjectId = targetId?.Trim() ?? string.Empty;
            resourcesReserved = false;
        }

        public void Clear()
        {
            kind = WorkerJobKind.None;
            targetObjectId = string.Empty;
            resourcesReserved = false;
        }
    }
}
