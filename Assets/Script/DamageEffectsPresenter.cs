using Assets.Script;
using UnityEngine;
using ZoneUA.Combat;

[DisallowMultipleComponent]
public sealed class DamageEffectsPresenter : MonoBehaviour
{
    [SerializeField] private DamageEffectSettings settings;
    [SerializeField, Tooltip("Optional component implementing IRuntimeObjectSpawner.")]
    private MonoBehaviour spawnerSource;

    private IRuntimeObjectSpawner spawner;

    private void Awake()
    {
        spawner = spawnerSource as IRuntimeObjectSpawner;
    }

    public void Present(in DamageInfo damageInfo, int appliedDamage)
    {
        if (settings == null || appliedDamage <= 0)
        {
            return;
        }

        Vector3 origin = damageInfo.HitPoint == Vector2.zero ? transform.position : damageInfo.HitPoint;
        SpawnParticle(origin);
        SpawnDecals(origin, damageInfo.HitDirection);
        SpawnPopup(origin);
    }

    private void SpawnParticle(Vector3 origin)
    {
        ParticleSystem prefab = settings.HitParticlePrefab;
        if (prefab == null)
        {
            return;
        }

        GameObject instanceObject = Spawn(prefab.gameObject, origin, prefab.transform.rotation);
        if (instanceObject == null || !instanceObject.TryGetComponent(out ParticleSystem instance))
        {
            return;
        }

        instance.Play();
        ParticleSystem.MainModule main = instance.main;
        float lifetime = main.duration + main.startLifetime.constantMax + settings.AdditionalParticleLifetime;
        ReleaseAfter(instanceObject, Mathf.Max(0.1f, lifetime));
    }

    private void SpawnDecals(Vector3 origin, Vector3 hitDirection)
    {
        GameObject[] prefabs = settings.DecalPrefabs;
        if (prefabs == null || prefabs.Length == 0)
        {
            return;
        }

        for (int i = 0; i < settings.DecalCount; i++)
        {
            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            if (prefab == null)
            {
                continue;
            }

            Vector2 offset = Random.insideUnitCircle * settings.SpawnRadius;
            Quaternion rotation = Quaternion.Euler(0f, 0f, Random.Range(-180f, 180f));
            GameObject instance = Spawn(prefab, origin + (Vector3)offset, rotation);
            if (instance == null || !instance.TryGetComponent(out Rigidbody2D body))
            {
                continue;
            }

            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            Vector2 direction = hitDirection.sqrMagnitude > 0.001f
                ? (Vector2)hitDirection.normalized
                : Random.insideUnitCircle.normalized;
            body.AddForce(direction * settings.ImpulseSpeed, ForceMode2D.Impulse);
            StartCoroutine(Tools.AttenuateVelocity(body, settings.ImpulseDuration));
        }
    }

    private void SpawnPopup(Vector3 origin)
    {
        Transform prefab = settings.DamagePopupPrefab;
        if (prefab == null)
        {
            return;
        }

        GameObject instance = Spawn(prefab.gameObject, origin, prefab.rotation);
        if (instance != null && settings.PopupLifetime > 0f)
        {
            ReleaseAfter(instance, settings.PopupLifetime);
        }
    }

    private GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        spawner ??= spawnerSource as IRuntimeObjectSpawner;
        return spawner != null
            ? spawner.Spawn(prefab, position, rotation)
            : Instantiate(prefab, position, rotation);
    }

    private void ReleaseAfter(GameObject instance, float delay)
    {
        if (spawner != null)
        {
            spawner.ReleaseAfter(instance, delay);
        }
        else
        {
            Destroy(instance, delay);
        }
    }
}
