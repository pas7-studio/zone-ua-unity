using System;
using System.Collections.Generic;
using System.IO;
using Unity.Profiling;
using UnityEngine;
using ZoneUA.Performance;

public sealed class RuntimePerformanceMonitor : MonoBehaviour
{
    [SerializeField] private PerformanceBudgetProfile budgetProfile;
    [SerializeField, Min(0.1f)] private float sampleInterval = 1f;
    [SerializeField] private bool showOverlay = true;
    [SerializeField] private bool writeJsonOnDisable;
    [SerializeField] private string outputFileName = "performance-capture.json";

    private readonly List<PerformanceSample> samples = new List<PerformanceSample>();
    private ProfilerRecorder mainThreadRecorder;
    private ProfilerRecorder renderThreadRecorder;
    private ProfilerRecorder gcAllocatedRecorder;
    private ProfilerRecorder reservedMemoryRecorder;
    private float timer;
    private PerformanceSample latest;

    [Serializable]
    private sealed class SampleCollection
    {
        public List<PerformanceSample> samples = new List<PerformanceSample>();
    }

    public PerformanceSample Latest => latest;
    public IReadOnlyList<PerformanceSample> Samples => samples;

    private void OnEnable()
    {
        mainThreadRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
        renderThreadRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Render Thread", 15);
        gcAllocatedRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 15);
        reservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Reserved Memory", 15);
    }

    private void Update()
    {
        timer += Time.unscaledDeltaTime;
        if (timer < sampleInterval) return;
        timer = 0f;
        CaptureSample();
    }

    private void OnDisable()
    {
        mainThreadRecorder.Dispose();
        renderThreadRecorder.Dispose();
        gcAllocatedRecorder.Dispose();
        reservedMemoryRecorder.Dispose();
        if (writeJsonOnDisable) WriteJson();
    }

    [ContextMenu("Capture Performance Sample")]
    public void CaptureSample()
    {
        GlobalSystem global = GlobalSystem.Instance;
        latest = new PerformanceSample
        {
            timestampSeconds = Time.realtimeSinceStartupAsDouble,
            framesPerSecond = Time.unscaledDeltaTime > 0f ? 1f / Time.unscaledDeltaTime : 0f,
            mainThreadMilliseconds = ToMilliseconds(mainThreadRecorder.LastValue),
            renderThreadMilliseconds = ToMilliseconds(renderThreadRecorder.LastValue),
            gcAllocatedBytes = gcAllocatedRecorder.LastValue,
            totalReservedMemoryBytes = reservedMemoryRecorder.LastValue,
            trackedPoolInstances = global != null ? global.TrackedInstanceCount : 0,
            scheduledPoolReleases = global != null ? global.ScheduledReleaseCount : 0,
            activeNpcCount = FindObjectsByType<NPCController>(FindObjectsSortMode.None).Length,
            activeProjectileCount = FindObjectsByType<Bullet>(FindObjectsSortMode.None).Length,
            generatedObjectCount = FindObjectsByType<ChunkManager>(FindObjectsSortMode.None).Length > 0
                ? CountGeneratedObjects()
                : 0
        };
        samples.Add(latest);
    }

    [ContextMenu("Write Performance JSON")]
    public void WriteJson()
    {
        string path = Path.Combine(Application.persistentDataPath, outputFileName);
        var collection = new SampleCollection { samples = new List<PerformanceSample>(samples) };
        File.WriteAllText(path, JsonUtility.ToJson(collection, true));
        Debug.Log($"Performance capture written to {path}", this);
    }

    private void OnGUI()
    {
        if (!showOverlay || budgetProfile == null) return;
        IReadOnlyList<PerformanceBudgetResult> results = PerformanceBudgetEvaluator.Evaluate(latest, budgetProfile);
        GUILayout.BeginArea(new Rect(10f, 10f, 360f, 420f), GUI.skin.box);
        GUILayout.Label("Zone UA Performance");
        foreach (PerformanceBudgetResult result in results)
        {
            GUILayout.Label($"{result.Status}: {result.Metric} {result.Value:0.##} / {result.Budget:0.##}");
        }
        GUILayout.EndArea();
    }

    private static float ToMilliseconds(long nanoseconds) => nanoseconds / 1_000_000f;

    private static int CountGeneratedObjects()
    {
        int count = 0;
        MapGenerator[] generators = FindObjectsByType<MapGenerator>(FindObjectsSortMode.None);
        foreach (MapGenerator generator in generators) count += generator.transform.childCount;
        return count;
    }
}
