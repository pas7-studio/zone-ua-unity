using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZoneUA.Infrastructure;

[DisallowMultipleComponent]
public sealed class RuntimeObjectPool : MonoBehaviour
{
    [SerializeField, Min(1), Tooltip("Maximum inactive instances retained for each prefab.")]
    private int maxInactivePerPrefab = 64;

    private sealed class Entry
    {
        public GameObject Prefab;
        public GameObject Instance;
        public Vector3 PrefabLocalScale;
        public readonly PoolLeaseState Lease = new PoolLeaseState();
        public Rigidbody2D[] Bodies2D;
        public Rigidbody[] Bodies3D;
        public TrailRenderer[] Trails;
        public ParticleSystem[] Particles;
        public IPoolable[] Callbacks;
    }

    private readonly Dictionary<GameObject, Queue<Entry>> availableByPrefab = new();
    private readonly Dictionary<GameObject, Entry> entryByInstance = new();
    private readonly Dictionary<GameObject, Coroutine> delayedReleaseByInstance = new();

    public int TrackedInstanceCount => entryByInstance.Count;
    public int ScheduledReleaseCount => delayedReleaseByInstance.Count;

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (prefab == null)
        {
            return null;
        }

        Queue<Entry> available = GetOrCreateQueue(prefab);
        Entry entry = null;

        while (available.Count > 0 && entry == null)
        {
            Entry candidate = available.Dequeue();
            if (candidate?.Instance == null)
            {
                continue;
            }

            entry = candidate;
        }

        if (entry == null)
        {
            GameObject instance = Instantiate(prefab);
            entry = CreateEntry(prefab, instance);
            entryByInstance.Add(instance, entry);
        }

        CancelScheduledRelease(entry.Instance);
        entry.Lease.Acquire();

        Transform instanceTransform = entry.Instance.transform;
        instanceTransform.SetParent(parent, false);
        instanceTransform.SetPositionAndRotation(position, rotation);
        instanceTransform.localScale = entry.PrefabLocalScale;

        ResetRuntimeState(entry);
        entry.Instance.SetActive(true);
        InvokeSpawned(entry);
        return entry.Instance;
    }

    public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        where T : Component
    {
        GameObject instance = Spawn(prefab != null ? prefab.gameObject : null, position, rotation, parent);
        return instance != null ? instance.GetComponent<T>() : null;
    }

    public bool Owns(GameObject instance)
    {
        return instance != null && entryByInstance.ContainsKey(instance);
    }

    public bool IsLeased(GameObject instance)
    {
        return instance != null &&
               entryByInstance.TryGetValue(instance, out Entry entry) &&
               entry.Lease.IsLeased;
    }

    public int GetLeaseGeneration(GameObject instance)
    {
        return instance != null && entryByInstance.TryGetValue(instance, out Entry entry)
            ? entry.Lease.Generation
            : 0;
    }

    public bool Release(GameObject instance)
    {
        if (instance == null || !entryByInstance.TryGetValue(instance, out Entry entry))
        {
            return false;
        }

        CancelScheduledRelease(instance);
        if (!entry.Lease.TryRelease())
        {
            return false;
        }

        ReturnEntry(entry);
        return true;
    }

    public bool ReleaseAfter(GameObject instance, float delay)
    {
        if (instance == null ||
            !entryByInstance.TryGetValue(instance, out Entry entry) ||
            !entry.Lease.IsLeased)
        {
            return false;
        }

        CancelScheduledRelease(instance);
        int expectedGeneration = entry.Lease.Generation;
        delayedReleaseByInstance[instance] = StartCoroutine(
            ReleaseAfterRoutine(entry, expectedGeneration, Mathf.Max(0f, delay)));
        return true;
    }

    public int GetInactiveCount(GameObject prefab)
    {
        return prefab != null && availableByPrefab.TryGetValue(prefab, out Queue<Entry> queue)
            ? queue.Count
            : 0;
    }

    public void Prewarm(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0)
        {
            return;
        }

        Queue<Entry> available = GetOrCreateQueue(prefab);
        int target = Mathf.Min(maxInactivePerPrefab, available.Count + count);

        while (available.Count < target)
        {
            GameObject instance = Instantiate(prefab, transform);
            Entry entry = CreateEntry(prefab, instance);
            entryByInstance.Add(instance, entry);
            ResetRuntimeState(entry);
            instance.SetActive(false);
            available.Enqueue(entry);
        }
    }

    public void Clear()
    {
        foreach (Coroutine routine in delayedReleaseByInstance.Values)
        {
            if (routine != null)
            {
                StopCoroutine(routine);
            }
        }

        delayedReleaseByInstance.Clear();

        foreach (Entry entry in entryByInstance.Values)
        {
            if (entry?.Instance != null)
            {
                Destroy(entry.Instance);
            }
        }

        availableByPrefab.Clear();
        entryByInstance.Clear();
    }

    private IEnumerator ReleaseAfterRoutine(Entry entry, int expectedGeneration, float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        GameObject instance = entry.Instance;
        if (instance != null)
        {
            delayedReleaseByInstance.Remove(instance);
        }

        if (instance == null || !entry.Lease.TryRelease(expectedGeneration))
        {
            yield break;
        }

        ReturnEntry(entry);
    }

    private void ReturnEntry(Entry entry)
    {
        GameObject instance = entry.Instance;
        if (instance == null)
        {
            return;
        }

        InvokeReleased(entry);
        ResetRuntimeState(entry);
        instance.SetActive(false);
        instance.transform.SetParent(transform, false);

        Queue<Entry> available = GetOrCreateQueue(entry.Prefab);
        if (available.Count >= maxInactivePerPrefab)
        {
            entryByInstance.Remove(instance);
            Destroy(instance);
            return;
        }

        available.Enqueue(entry);
    }

    private void CancelScheduledRelease(GameObject instance)
    {
        if (instance == null || !delayedReleaseByInstance.TryGetValue(instance, out Coroutine routine))
        {
            return;
        }

        if (routine != null)
        {
            StopCoroutine(routine);
        }

        delayedReleaseByInstance.Remove(instance);
    }

    private Queue<Entry> GetOrCreateQueue(GameObject prefab)
    {
        if (!availableByPrefab.TryGetValue(prefab, out Queue<Entry> queue))
        {
            queue = new Queue<Entry>();
            availableByPrefab.Add(prefab, queue);
        }

        return queue;
    }

    private static Entry CreateEntry(GameObject prefab, GameObject instance)
    {
        MonoBehaviour[] behaviours = instance.GetComponentsInChildren<MonoBehaviour>(true);
        var callbacks = new List<IPoolable>(behaviours.Length);
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IPoolable poolable)
            {
                callbacks.Add(poolable);
            }
        }

        return new Entry
        {
            Prefab = prefab,
            Instance = instance,
            PrefabLocalScale = prefab.transform.localScale,
            Bodies2D = instance.GetComponentsInChildren<Rigidbody2D>(true),
            Bodies3D = instance.GetComponentsInChildren<Rigidbody>(true),
            Trails = instance.GetComponentsInChildren<TrailRenderer>(true),
            Particles = instance.GetComponentsInChildren<ParticleSystem>(true),
            Callbacks = callbacks.ToArray()
        };
    }

    private static void ResetRuntimeState(Entry entry)
    {
        for (int i = 0; i < entry.Bodies2D.Length; i++)
        {
            Rigidbody2D body = entry.Bodies2D[i];
            if (body == null) continue;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }

        for (int i = 0; i < entry.Bodies3D.Length; i++)
        {
            Rigidbody body = entry.Bodies3D[i];
            if (body == null) continue;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        for (int i = 0; i < entry.Trails.Length; i++)
        {
            entry.Trails[i]?.Clear();
        }

        for (int i = 0; i < entry.Particles.Length; i++)
        {
            ParticleSystem particle = entry.Particles[i];
            if (particle != null)
            {
                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }

    private static void InvokeSpawned(Entry entry)
    {
        for (int i = 0; i < entry.Callbacks.Length; i++)
        {
            entry.Callbacks[i]?.OnPoolSpawned();
        }
    }

    private static void InvokeReleased(Entry entry)
    {
        for (int i = 0; i < entry.Callbacks.Length; i++)
        {
            entry.Callbacks[i]?.OnPoolReleased();
        }
    }

    private void OnDestroy()
    {
        Clear();
    }

    private void OnValidate()
    {
        maxInactivePerPrefab = Mathf.Max(1, maxInactivePerPrefab);
    }
}
