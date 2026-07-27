using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class RuntimeObjectPool : MonoBehaviour
{
    private readonly Dictionary<GameObject, Queue<GameObject>> availableByPrefab = new();
    private readonly Dictionary<GameObject, GameObject> prefabByInstance = new();

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (prefab == null)
        {
            return null;
        }

        if (!availableByPrefab.TryGetValue(prefab, out Queue<GameObject> available))
        {
            available = new Queue<GameObject>();
            availableByPrefab.Add(prefab, available);
        }

        GameObject instance = null;
        while (available.Count > 0 && instance == null)
        {
            instance = available.Dequeue();
        }

        if (instance == null)
        {
            instance = Instantiate(prefab);
            prefabByInstance[instance] = prefab;
        }

        Transform instanceTransform = instance.transform;
        instanceTransform.SetParent(parent, false);
        instanceTransform.SetPositionAndRotation(position, rotation);
        ResetPhysics(instance);
        instance.SetActive(true);
        return instance;
    }

    public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        where T : Component
    {
        GameObject instance = Spawn(prefab != null ? prefab.gameObject : null, position, rotation, parent);
        return instance != null ? instance.GetComponent<T>() : null;
    }

    public bool Release(GameObject instance)
    {
        if (instance == null || !prefabByInstance.TryGetValue(instance, out GameObject prefab))
        {
            return false;
        }

        ResetPhysics(instance);
        instance.SetActive(false);
        instance.transform.SetParent(transform, false);

        if (!availableByPrefab.TryGetValue(prefab, out Queue<GameObject> available))
        {
            available = new Queue<GameObject>();
            availableByPrefab.Add(prefab, available);
        }

        available.Enqueue(instance);
        return true;
    }

    public void ReleaseAfter(GameObject instance, float delay)
    {
        if (instance == null)
        {
            return;
        }

        StartCoroutine(ReleaseAfterRoutine(instance, Mathf.Max(0f, delay)));
    }

    public void Clear()
    {
        foreach (KeyValuePair<GameObject, GameObject> entry in prefabByInstance)
        {
            if (entry.Key != null)
            {
                Destroy(entry.Key);
            }
        }

        availableByPrefab.Clear();
        prefabByInstance.Clear();
    }

    private System.Collections.IEnumerator ReleaseAfterRoutine(GameObject instance, float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        Release(instance);
    }

    private static void ResetPhysics(GameObject instance)
    {
        if (instance.TryGetComponent(out Rigidbody2D body2D))
        {
            body2D.velocity = Vector2.zero;
            body2D.angularVelocity = 0f;
        }

        if (instance.TryGetComponent(out Rigidbody body3D))
        {
            body3D.velocity = Vector3.zero;
            body3D.angularVelocity = Vector3.zero;
        }

        if (instance.TryGetComponent(out TrailRenderer trail))
        {
            trail.Clear();
        }

        ParticleSystem[] particles = instance.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
