using System.Collections.Generic;

namespace ZoneUA.Performance
{
    public enum PerformanceBudgetStatus
    {
        Pass,
        Warning,
        Fail
    }

    public readonly struct PerformanceBudgetResult
    {
        public PerformanceBudgetResult(string metric, double value, double budget, PerformanceBudgetStatus status)
        {
            Metric = metric;
            Value = value;
            Budget = budget;
            Status = status;
        }

        public string Metric { get; }
        public double Value { get; }
        public double Budget { get; }
        public PerformanceBudgetStatus Status { get; }
    }

    public static class PerformanceBudgetEvaluator
    {
        public static IReadOnlyList<PerformanceBudgetResult> Evaluate(
            PerformanceSample sample,
            PerformanceBudgetProfile profile,
            float warningRatio = 0.85f)
        {
            var results = new List<PerformanceBudgetResult>(10);
            if (profile == null)
            {
                return results;
            }

            warningRatio = UnityEngine.Mathf.Clamp01(warningRatio);
            AddMaximum(results, "Main Thread ms", sample.mainThreadMilliseconds, profile.MaximumMainThreadMilliseconds, warningRatio);
            AddMaximum(results, "Render Thread ms", sample.renderThreadMilliseconds, profile.MaximumRenderThreadMilliseconds, warningRatio);
            AddMaximum(results, "GC Allocated Bytes", sample.gcAllocatedBytes, profile.MaximumGcAllocatedBytesPerFrame, warningRatio);
            AddMaximum(results, "Reserved Memory Bytes", sample.totalReservedMemoryBytes, profile.MaximumTotalReservedMemoryBytes, warningRatio);
            AddMaximum(results, "Tracked Pool Instances", sample.trackedPoolInstances, profile.MaximumTrackedPoolInstances, warningRatio);
            AddMaximum(results, "Scheduled Pool Releases", sample.scheduledPoolReleases, profile.MaximumScheduledPoolReleases, warningRatio);
            AddMaximum(results, "Active NPC", sample.activeNpcCount, profile.MaximumActiveNpcCount, warningRatio);
            AddMaximum(results, "Active Projectiles", sample.activeProjectileCount, profile.MaximumActiveProjectileCount, warningRatio);
            AddMaximum(results, "Generated Objects", sample.generatedObjectCount, profile.MaximumGeneratedObjectCount, warningRatio);
            AddMinimum(results, "FPS", sample.framesPerSecond, profile.TargetFramesPerSecond, warningRatio);
            return results;
        }

        private static void AddMaximum(List<PerformanceBudgetResult> results, string metric, double value, double budget, float warningRatio)
        {
            if (budget <= 0d)
            {
                results.Add(new PerformanceBudgetResult(metric, value, budget, PerformanceBudgetStatus.Pass));
                return;
            }

            PerformanceBudgetStatus status = value > budget
                ? PerformanceBudgetStatus.Fail
                : value >= budget * warningRatio
                    ? PerformanceBudgetStatus.Warning
                    : PerformanceBudgetStatus.Pass;
            results.Add(new PerformanceBudgetResult(metric, value, budget, status));
        }

        private static void AddMinimum(List<PerformanceBudgetResult> results, string metric, double value, double budget, float warningRatio)
        {
            if (budget <= 0d)
            {
                results.Add(new PerformanceBudgetResult(metric, value, budget, PerformanceBudgetStatus.Pass));
                return;
            }

            PerformanceBudgetStatus status = value < budget * warningRatio
                ? PerformanceBudgetStatus.Fail
                : value < budget
                    ? PerformanceBudgetStatus.Warning
                    : PerformanceBudgetStatus.Pass;
            results.Add(new PerformanceBudgetResult(metric, value, budget, status));
        }
    }
}
