using System;

namespace ZoneUA.Performance
{
    [Serializable]
    public struct PerformanceSample
    {
        public double timestampSeconds;
        public float framesPerSecond;
        public float mainThreadMilliseconds;
        public float renderThreadMilliseconds;
        public long gcAllocatedBytes;
        public long totalReservedMemoryBytes;
        public int trackedPoolInstances;
        public int scheduledPoolReleases;
        public int activeNpcCount;
        public int activeProjectileCount;
        public int generatedObjectCount;
    }
}
