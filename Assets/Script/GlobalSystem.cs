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

    public Vector2 WeaponSpawnOffset => new Vector2(weaponXOffset * 10f, weaponYOffset * 10f);
    public int BloodAmount => bloodAmount;
    public float BloodSpawnRadius => spawnRadius;
    public float BloodImpulseSpeed => bloodImpulsSpeed;
    public float BloodImpulseDuration => bloodImpulseDuration;
    public ParticleSystem BloodParticleSystem => bloodParticleSystem;
    public Transform RuntimeContainer => runtimeContainer;
    public UIAmmoSystem AmmoUI => ammoUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"Only one {nameof(GlobalSystem)} is allowed in a scene.", this);
            enabled = false;
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public bool TryGetRandomBlood(out GameObject prefab)
    {
        prefab = null;

        if (bloodPrefabs == null || bloodPrefabs.Length == 0)
        {
            return false;
        }

        prefab = bloodPrefabs[Random.Range(0, bloodPrefabs.Length)];
        return prefab != null;
    }

    // Kept for compatibility with existing scene events and older code.
    public GameObject getRandomBlood()
    {
        TryGetRandomBlood(out GameObject prefab);
        return prefab;
    }

    private void OnValidate()
    {
        bloodAmount = Mathf.Max(0, bloodAmount);
        spawnRadius = Mathf.Max(0f, spawnRadius);
        bloodImpulsSpeed = Mathf.Max(0f, bloodImpulsSpeed);
        bloodImpulseDuration = Mathf.Max(0f, bloodImpulseDuration);
    }
}
