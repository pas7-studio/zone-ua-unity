using System;
using UnityEngine;
using UnityEngine.Serialization;

[DefaultExecutionOrder(-1000)]
[DisallowMultipleComponent]
public sealed class GlobalSystem : MonoBehaviour
{
    public static GlobalSystem Instance { get; private set; }

    [Header("Weapon")]
    [SerializeField, Min(0f)] private float weaponXOffset = 0.005f;
    [SerializeField, Min(0f)] private float weaponYOffset = 0.035f;

    [Header("Blood")]
    [SerializeField, Min(0)] private int bloodAmount = 10;
    [SerializeField, Min(0f)] private float spawnRadius = 1f;
    [SerializeField, Min(0f)] private float bloodImpulsSpeed = 1f;
    [SerializeField, Min(0f)] private float bloodImpulseDuration = 1f;
    [SerializeField] private GameObject[] bloodPrefabs;
    [SerializeField] private ParticleSystem bloodParticleSystem;

    [Header("Scene References")]
    [FormerlySerializedAs("garbadge")]
    [SerializeField] private Transform runtimeContainer;

    [FormerlySerializedAs("UIAmmoSystem")]
    [SerializeField] private UIAmmoSystem ammoUI;

    private RuntimeObjectPool objectPool;

    public Vector2 WeaponSpawnOffset => new Vector2(weaponXOffset * 10f, weaponYOffset * 10f);
    public int BloodAmount => bloodAmount;
    public float BloodSpawnRadius => spawnRadius;
    public float BloodImpulseSpeed => bloodImpulsSpeed;
    public float BloodImpulseDuration => bloodImpulseDuration;
    public ParticleSystem BloodParticleSystem => bloodParticleSystem;
    public Transform RuntimeContainer => runtimeContainer;

    [Obsolete("Gameplay code must use WeaponAmmoPresenter events instead of accessing UI through GlobalSystem.")]
    public UIAmmoSystem AmmoUI => ammoUI;

    public RuntimeObjectPool ObjectPool => objectPool;
    public int TrackedInstanceCount => objectPool != null ? objectPool.TrackedInstanceCount : 0;
    public int ScheduledReleaseCount => objectPool != null ? objectPool.ScheduledReleaseCount : 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"Only one {nameof(GlobalSystem)} is allowed in a scene.", this);
            enabled = false;
            return;
        }

        Instance = this;
        EnsureRuntimeInfrastructure();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (prefab == null) return null;
        EnsureRuntimeInfrastructure();
        return objectPool.Spawn(prefab, position, rotation, parent);
    }

    public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        where T : Component
    {
        if (prefab == null) return null;
        EnsureRuntimeInfrastructure();
        return objectPool.Spawn(prefab, position, rotation, parent);
    }

    public void Release(GameObject instance)
    {
        if (instance == null) return;

        EnsureRuntimeInfrastructure();
        if (objectPool.Owns(instance))
        {
            objectPool.Release(instance);
            return;
        }

        Destroy(instance);
    }

    public void ReleaseAfter(GameObject instance, float delay)
    {
        if (instance == null) return;

        EnsureRuntimeInfrastructure();
        if (objectPool.Owns(instance))
        {
            objectPool.ReleaseAfter(instance, delay);
            return;
        }

        Destroy(instance, Mathf.Max(0f, delay));
    }

    public void Prewarm(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0) return;
        EnsureRuntimeInfrastructure();
        objectPool.Prewarm(prefab, count);
    }

    public bool Owns(GameObject instance)
    {
        EnsureRuntimeInfrastructure();
        return objectPool.Owns(instance);
    }

    public bool TryGetRandomBlood(out GameObject prefab)
    {
        prefab = null;
        if (bloodPrefabs == null || bloodPrefabs.Length == 0) return false;

        prefab = bloodPrefabs[Random.Range(0, bloodPrefabs.Length)];
        return prefab != null;
    }

    [Obsolete("Use TryGetRandomBlood(out GameObject) and handle an empty configuration explicitly.")]
    public GameObject getRandomBlood()
    {
        TryGetRandomBlood(out GameObject prefab);
        return prefab;
    }

    private void EnsureRuntimeInfrastructure()
    {
        if (runtimeContainer == null)
        {
            GameObject container = new GameObject("Runtime Objects");
            container.transform.SetParent(transform, false);
            runtimeContainer = container.transform;
        }

        objectPool = runtimeContainer.GetComponent<RuntimeObjectPool>();
        if (objectPool == null)
        {
            objectPool = runtimeContainer.gameObject.AddComponent<RuntimeObjectPool>();
        }
    }

    private void OnValidate()
    {
        bloodAmount = Mathf.Max(0, bloodAmount);
        spawnRadius = Mathf.Max(0f, spawnRadius);
        bloodImpulsSpeed = Mathf.Max(0f, bloodImpulsSpeed);
        bloodImpulseDuration = Mathf.Max(0f, bloodImpulseDuration);
    }
}
