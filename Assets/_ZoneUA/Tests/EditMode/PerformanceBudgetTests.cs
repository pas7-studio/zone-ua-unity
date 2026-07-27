using System.Linq;
using NUnit.Framework;
using UnityEngine;
using ZoneUA.Performance;

namespace ZoneUA.Combat.Tests
{
    public sealed class PerformanceBudgetTests
    {
        [Test]
        public void Evaluate_PassingSample_ReturnsNoFailures()
        {
            PerformanceBudgetProfile profile = ScriptableObject.CreateInstance<PerformanceBudgetProfile>();
            var sample = new PerformanceSample
            {
                framesPerSecond = 60f,
                mainThreadMilliseconds = 5f,
                renderThreadMilliseconds = 5f,
                gcAllocatedBytes = 0,
                totalReservedMemoryBytes = 128 * 1024 * 1024,
                trackedPoolInstances = 10,
                scheduledPoolReleases = 2,
                activeNpcCount = 5,
                activeProjectileCount = 10,
                generatedObjectCount = 100
            };

            var results = PerformanceBudgetEvaluator.Evaluate(sample, profile);

            Assert.That(results.Any(result => result.Status == PerformanceBudgetStatus.Fail), Is.False);
            Object.DestroyImmediate(profile);
        }

        [Test]
        public void Evaluate_ExceededFrameAndFpsBudgets_ReturnsFailures()
        {
            PerformanceBudgetProfile profile = ScriptableObject.CreateInstance<PerformanceBudgetProfile>();
            var sample = new PerformanceSample
            {
                framesPerSecond = 20f,
                mainThreadMilliseconds = 40f
            };

            var results = PerformanceBudgetEvaluator.Evaluate(sample, profile);

            Assert.That(results.Single(result => result.Metric == "FPS").Status, Is.EqualTo(PerformanceBudgetStatus.Fail));
            Assert.That(results.Single(result => result.Metric == "Main Thread ms").Status, Is.EqualTo(PerformanceBudgetStatus.Fail));
            Object.DestroyImmediate(profile);
        }

        [Test]
        public void Evaluate_NullProfile_ReturnsEmptyResult()
        {
            var results = PerformanceBudgetEvaluator.Evaluate(default, null);
            Assert.That(results, Is.Empty);
        }
    }
}
