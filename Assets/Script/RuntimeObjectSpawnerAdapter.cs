using UnityEngine;
using ZoneUA.Combat;

[DisallowMultipleComponent]
public sealed class RuntimeObjectSpawnerAdapter : MonoBehaviour, IRuntimeObjectSpawner
{
    [SerializeField, Tooltip("Optional explicit GlobalSystem reference. Scene instance is used as fallback.")]
    private GlobalSystem globalSystem;

    private void Awake()
    {
        globalSystem ??= GlobalSystem.Instance;
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        globalSystem ??= GlobalSystem.Instance;
        return globalSystem != null
            ? globalSystem.Spawn(prefab, position, rotation, parent)
            : Instantiate(prefab, position, rotation, parent);
    }

    public void Release(GameObject instance)
    {
        if (instance == null)
        {
            return;
        }

        globalSystem ??= GlobalSystem.Instance;
        if (globalSystem != null)
        {
            globalSystem.Release(instance);
        }
        else
        {
            Destroy(instance);
        }
    }

    public void ReleaseAfter(GameObject instance, float delay)
    {
        if (instance == null)
        {
            return;
        }

        globalSystem ??= GlobalSystem.Instance;
        if (globalSystem != null)
        {
            globalSystem.ReleaseAfter(instance, delay);
        }
        else
        {
            Destroy(instance, Mathf.Max(0f, delay));
        }
    }
}
