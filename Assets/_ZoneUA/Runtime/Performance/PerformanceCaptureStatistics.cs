using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneUA.Performance
{
    public readonly struct PerformanceCaptureStatistics
    {
        public PerformanceCaptureStatistics(
            int sampleCount,
            float averageFramesPerSecond,
            float p95MainThreadMilliseconds,
            float p95RenderThreadMilliseconds,
            long maximumGcAllocatedBytes,
            long maximumReservedMemoryBytes)
        {
            SampleCount = sampleCount;
            AverageFramesPerSecond = averageFramesPerSecond;
            P95MainThreadMilliseconds = p95MainThreadMilliseconds;
            P95RenderThreadMilliseconds = p95RenderThreadMilliseconds;
            MaximumGcAllocatedBytes = maximumGcAllocatedBytes;
            MaximumReservedMemoryBytes = maximumReservedMemoryBytes;
        }

        public int SampleCount { get; }
        public float AverageFramesPerSecond { get; }
        public float P95MainThreadMilliseconds { get; }
        public float P95RenderThreadMilliseconds { get; }
        public long MaximumGcAllocatedBytes { get; }
        public long MaximumReservedMemoryBytes { get; }

        public static PerformanceCaptureStatistics From(IReadOnlyList<PerformanceSample> samples)
        {
            if (samples == null || samples.Count == 0)
            {
                return default;
            }

            float averageFps = samples.Average(sample => sample.framesPerSecond);
            float p95Main = Percentile(samples.Select(sample => sample.mainThreadMilliseconds), 0.95f);
            float p95Render = Percentile(samples.Select(sample => sample.renderThreadMilliseconds), 0.95f);
            long maxGc = samples.Max(sample => sample.gcAllocatedBytes);
            long maxMemory = samples.Max(sample => sample.totalReservedMemoryBytes);

            return new PerformanceCaptureStatistics(
                samples.Count,
                averageFps,
                p95Main,
                p95Render,
                maxGc,
                maxMemory);
        }

        private static float Percentile(IEnumerable<float> values, float percentile)
        {
            float[] ordered = values.OrderBy(value => value).ToArray();
            if (ordered.Length == 0)
            {
                return 0f;
            }

            percentile = Math.Max(0f, Math.Min(1f, percentile));
            int index = (int)Math.Ceiling(percentile * ordered.Length) - 1;
            index = Math.Max(0, Math.Min(ordered.Length - 1, index));
            return ordered[index];
        }
    }
}
