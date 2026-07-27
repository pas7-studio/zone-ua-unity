using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Unity.Profiling;
using UnityEngine;
using ZoneUA.Performance;

public sealed class RuntimePerformanceMonitor : MonoBehaviour
{
    [SerializeField] private PerformanceBudgetProfile budgetProfile;
    [SerializeField, Min(0.1f)] private float sampleInterval = 1f;
    [SerializeField] private bool showOverlay = true;
    [SerializeField] private bool writeReportsOnDisable;
    [SerializeField] private string outputBaseName = "performance-capture";

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
        if (writeReportsOnDisable) WriteReports();
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

    [ContextMenu("Write Performance Reports")]
    public void WriteReports()
    {
        WriteJson();
        WriteCsv();
    }

    public void WriteJson()
    {
        string path = GetOutputPath("json");
        var collection = new SampleCollection { samples = new List<PerformanceSample>(samples) };
        File.WriteAllText(path, JsonUtility.ToJson(collection, true));
        Debug.Log($"Performance JSON written to {path}", this);
    }

    public void WriteCsv()
    {
        string path = GetOutputPath("csv");
        var builder = new StringBuilder();
        builder.AppendLine("timestampSeconds,fps,mainThreadMs,renderThreadMs,gcAllocatedBytes,reservedMemoryBytes,trackedPoolInstances,scheduledPoolReleases,activeNpcCount,activeProjectileCount,generatedObjectCount");
        foreach (PerformanceSample sample in samples)
        {
            builder.Append(sample.timestampSeconds.ToString("0.###", CultureInfo.InvariantCulture)).Append(',')
                .Append(sample.framesPerSecond.ToString("0.###", CultureInfo.InvariantCulture)).Append(',')
                .Append(sample.mainThreadMilliseconds.ToString("0.###", CultureInfo.InvariantCulture)).Append(',')
                .Append(sample.renderThreadMilliseconds.ToString("0.###", CultureInfo.InvariantCulture)).Append(',')
                .Append(sample.gcAllocatedBytes).Append(',')
                .Append(sample.totalReservedMemoryBytes).Append(',')
                .Append(sample.trackedPoolInstances).Append(',')
                .Append(sample.scheduledPoolReleases).Append(',')
                .Append(sample.activeNpcCount).Append(',')
                .Append(sample.activeProjectileCount).Append(',')
                .Append(sample.generatedObjectCount).AppendLine();
        }
        File.WriteAllText(path, builder.ToString());
        Debug.Log($"Performance CSV written to {path}", this);
    }

    private string GetOutputPath(string extension)
    {
        string safeBaseName = string.IsNullOrWhiteSpace(outputBaseName) ? "performance-capture" : outputBaseName.Trim();
        return Path.Combine(Application.persistentDataPath, $"{safeBaseName}.{extension}");
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
