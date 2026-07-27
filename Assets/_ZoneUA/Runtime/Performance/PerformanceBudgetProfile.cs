using UnityEngine;

namespace ZoneUA.Performance
{
    [CreateAssetMenu(fileName = "PerformanceBudgetProfile", menuName = "Zone UA/Performance/Budget Profile")]
    public sealed class PerformanceBudgetProfile : ScriptableObject
    {
        [Header("Frame")]
        [SerializeField, Min(1f)] private float targetFramesPerSecond = 60f;
        [SerializeField, Min(0f)] private float maximumMainThreadMilliseconds = 16.67f;
        [SerializeField, Min(0f)] private float maximumRenderThreadMilliseconds = 16.67f;
        [SerializeField, Min(0)] private long maximumGcAllocatedBytesPerFrame = 1024;

        [Header("Runtime Objects")]
        [SerializeField, Min(0)] private int maximumTrackedPoolInstances = 2000;
        [SerializeField, Min(0)] private int maximumScheduledPoolReleases = 500;
        [SerializeField, Min(0)] private int maximumActiveNpcCount = 150;
        [SerializeField, Min(0)] private int maximumActiveProjectileCount = 500;
        [SerializeField, Min(0)] private int maximumGeneratedObjectCount = 10000;

        [Header("Memory")]
        [SerializeField, Min(0)] private long maximumTotalReservedMemoryBytes = 2L * 1024L * 1024L * 1024L;

        public float TargetFramesPerSecond => targetFramesPerSecond;
        public float MaximumMainThreadMilliseconds => maximumMainThreadMilliseconds;
        public float MaximumRenderThreadMilliseconds => maximumRenderThreadMilliseconds;
        public long MaximumGcAllocatedBytesPerFrame => maximumGcAllocatedBytesPerFrame;
        public int MaximumTrackedPoolInstances => maximumTrackedPoolInstances;
        public int MaximumScheduledPoolReleases => maximumScheduledPoolReleases;
        public int MaximumActiveNpcCount => maximumActiveNpcCount;
        public int MaximumActiveProjectileCount => maximumActiveProjectileCount;
        public int MaximumGeneratedObjectCount => maximumGeneratedObjectCount;
        public long MaximumTotalReservedMemoryBytes => maximumTotalReservedMemoryBytes;

        private void OnValidate()
        {
            targetFramesPerSecond = Mathf.Max(1f, targetFramesPerSecond);
            maximumMainThreadMilliseconds = Mathf.Max(0f, maximumMainThreadMilliseconds);
            maximumRenderThreadMilliseconds = Mathf.Max(0f, maximumRenderThreadMilliseconds);
            maximumGcAllocatedBytesPerFrame = System.Math.Max(0L, maximumGcAllocatedBytesPerFrame);
            maximumTrackedPoolInstances = Mathf.Max(0, maximumTrackedPoolInstances);
            maximumScheduledPoolReleases = Mathf.Max(0, maximumScheduledPoolReleases);
            maximumActiveNpcCount = Mathf.Max(0, maximumActiveNpcCount);
            maximumActiveProjectileCount = Mathf.Max(0, maximumActiveProjectileCount);
            maximumGeneratedObjectCount = Mathf.Max(0, maximumGeneratedObjectCount);
            maximumTotalReservedMemoryBytes = System.Math.Max(0L, maximumTotalReservedMemoryBytes);
        }
    }
}
