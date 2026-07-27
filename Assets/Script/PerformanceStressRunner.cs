using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZoneUA.Performance;

public sealed class PerformanceStressRunner : MonoBehaviour
{
    [SerializeField] private StressScenarioDefinition scenario;
    [SerializeField] private RuntimePerformanceMonitor monitor;
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform spawnRoot;
    [SerializeField, Min(0.1f)] private float spawnRadius = 20f;
    [SerializeField] private bool runOnStart;

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();
    private Coroutine routine;

    public bool IsRunning => routine != null;

    private void Start()
    {
        if (runOnStart) Run();
    }

    [ContextMenu("Run Stress Scenario")]
    public void Run()
    {
        if (routine != null || scenario == null) return;
        routine = StartCoroutine(RunRoutine());
    }

    [ContextMenu("Stop Stress Scenario")]
    public void Stop()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
        CleanupSpawnedObjects();
    }

    private IEnumerator RunRoutine()
    {
        if (scenario.RegenerateWorldBeforeCapture && mapGenerator != null)
        {
            mapGenerator.Regenerate();
        }

        SpawnNpcLoad(scenario.RequestedNpcCount);

        float warmupEnd = Time.realtimeSinceStartup + scenario.WarmupSeconds;
        while (Time.realtimeSinceStartup < warmupEnd)
        {
            SpawnProjectileLoad(Time.unscaledDeltaTime);
            yield return null;
        }

        float captureEnd = Time.realtimeSinceStartup + scenario.CaptureSeconds;
        while (Time.realtimeSinceStartup < captureEnd)
        {
            SpawnProjectileLoad(Time.unscaledDeltaTime);
            monitor?.CaptureSample();
            yield return null;
        }

        monitor?.WriteJson();
        CleanupSpawnedObjects();
        routine = null;
    }

    private void SpawnNpcLoad(int count)
    {
        if (npcPrefab == null) return;
        for (int i = 0; i < count; i++)
        {
            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            SpawnTracked(npcPrefab, transform.position + (Vector3)offset, Quaternion.identity);
        }
    }

    private void SpawnProjectileLoad(float deltaTime)
    {
        if (projectilePrefab == null || scenario.RequestedProjectileBurstsPerSecond <= 0) return;
        float expected = scenario.RequestedProjectileBurstsPerSecond * deltaTime;
        int count = Mathf.FloorToInt(expected);
        if (Random.value < expected - count) count++;

        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(0f, 360f);
            SpawnTracked(projectilePrefab, transform.position, Quaternion.Euler(0f, 0f, angle));
        }
    }

    private void SpawnTracked(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject instance = GlobalSystem.Instance != null
            ? GlobalSystem.Instance.Spawn(prefab, position, rotation, spawnRoot)
            : Instantiate(prefab, position, rotation, spawnRoot);
        if (instance != null) spawnedObjects.Add(instance);
    }

    private void CleanupSpawnedObjects()
    {
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            GameObject instance = spawnedObjects[i];
            if (instance == null) continue;
            if (GlobalSystem.Instance != null && GlobalSystem.Instance.Owns(instance))
                GlobalSystem.Instance.Release(instance);
            else
                Destroy(instance);
        }
        spawnedObjects.Clear();
    }

    private void OnDisable() => Stop();
}
