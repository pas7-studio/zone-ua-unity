using Assets.Script;
using System;
using UnityEngine;
using ZoneUA.Combat;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Death))]
public sealed class Health : MonoBehaviour, IDamageable
{
    [Header("Health Configuration")]
    [SerializeField, Min(1), Tooltip("Maximum health for this character.")]
    private int defaultHeals = 100;

    [SerializeField, Tooltip("Ignores incoming damage while enabled.")]
    private bool isImunable;

    [Header("Runtime State (Read Only)")]
    [SerializeField, HideInInspector] private int currentHeals = 100;
    [SerializeField, HideInInspector] private bool isAlive = true;

    private Death death;
    private GlobalSystem globalSystem;

    public event Action<DamageInfo> Damaged;
    public event Action<int, int> HealthChanged;
    public event Action<int> Healed;
    public event Action Died;

    public int CurrentHealth => currentHeals;
    public int MaximumHealth => defaultHeals;
    public bool IsAlive => isAlive;
    public bool IsImmune => isImunable;

    private void Awake()
    {
        death = GetComponent<Death>();
        currentHeals = Mathf.Clamp(currentHeals, 0, defaultHeals);
        isAlive = currentHeals > 0;
    }

    private void Start()
    {
        globalSystem = GlobalSystem.Instance;

        if (!isAlive)
        {
            Die();
        }
    }

    public void SetHealth(int health)
    {
        if (!isAlive)
        {
            return;
        }

        int previousHealth = currentHeals;
        currentHeals = Mathf.Clamp(health, 0, defaultHeals);
        NotifyHealthChanged(previousHealth);

        if (currentHeals == 0)
        {
            Die();
        }
    }

    public void RestoreHealth(int amount)
    {
        if (!isAlive || amount <= 0)
        {
            return;
        }

        int previousHealth = currentHeals;
        currentHeals = Mathf.Clamp(currentHeals + amount, 0, defaultHeals);
        int restoredAmount = currentHeals - previousHealth;

        if (restoredAmount <= 0)
        {
            return;
        }

        Healed?.Invoke(restoredAmount);
        NotifyHealthChanged(previousHealth);
    }

    public void RestoreFullHealth()
    {
        RestoreHealth(defaultHeals - currentHeals);
    }

    public void ReceiveDamage(in DamageInfo damageInfo)
    {
        if (!isAlive || isImunable || damageInfo.Amount <= 0)
        {
            return;
        }

        int previousHealth = currentHeals;
        currentHeals = Mathf.Max(0, currentHeals - damageInfo.Amount);

        Damaged?.Invoke(damageInfo);
        NotifyHealthChanged(previousHealth);
        SpawnBlood();

        if (currentHeals == 0)
        {
            Die();
        }
    }

    public void ReceiveDamage(int damageAmount)
    {
        DamageInfo damageInfo = new DamageInfo(
            damageAmount,
            null,
            null,
            transform.position,
            Vector2.zero,
            DamageType.Environment);

        ReceiveDamage(in damageInfo);
    }

    private void NotifyHealthChanged(int previousHealth)
    {
        if (previousHealth != currentHeals)
        {
            HealthChanged?.Invoke(currentHeals, defaultHeals);
        }
    }

    private void Die()
    {
        if (death == null || death.IsDead)
        {
            return;
        }

        isAlive = false;
        death.Dead();
        Died?.Invoke();
    }

    private void SpawnBlood()
    {
        globalSystem ??= GlobalSystem.Instance;

        if (globalSystem == null)
        {
            return;
        }

        SpawnBloodEffect();

        for (int i = 0; i < globalSystem.BloodAmount; i++)
        {
            if (!globalSystem.TryGetRandomBlood(out GameObject bloodPrefab))
            {
                return;
            }

            Vector2 spawnPosition =
                (Vector2)transform.position +
                UnityEngine.Random.insideUnitCircle * globalSystem.BloodSpawnRadius;

            Quaternion rotation = Quaternion.Euler(
                0f,
                0f,
                UnityEngine.Random.Range(-180f, 180f));

            GameObject bloodDrop = globalSystem.Spawn(
                bloodPrefab,
                spawnPosition,
                rotation,
                globalSystem.RuntimeContainer);

            if (bloodDrop == null || !bloodDrop.TryGetComponent(out Rigidbody2D body))
            {
                continue;
            }

            Vector2 forceDirection =
                UnityEngine.Random.insideUnitCircle.normalized * globalSystem.BloodImpulseSpeed;

            body.AddForce(forceDirection, ForceMode2D.Impulse);
            StartCoroutine(Tools.AttenuateVelocity(body, globalSystem.BloodImpulseDuration));
        }
    }

    private void SpawnBloodEffect()
    {
        ParticleSystem bloodEffectPrefab = globalSystem.BloodParticleSystem;
        if (bloodEffectPrefab == null)
        {
            return;
        }

        ParticleSystem instance = globalSystem.Spawn(
            bloodEffectPrefab,
            transform.position,
            bloodEffectPrefab.transform.rotation,
            globalSystem.RuntimeContainer);

        if (instance == null)
        {
            return;
        }

        instance.Play();

        ParticleSystem.MainModule main = instance.main;
        float lifetime = main.duration + main.startLifetime.constantMax;
        globalSystem.ReleaseAfter(instance.gameObject, Mathf.Max(0.1f, lifetime));
    }

    public void HealthLogic()
    {
        if (currentHeals <= 0)
        {
            Die();
        }
    }

    // Compatibility API retained until scenes, prefabs and UnityEvents are audited.
    public void setHeals(int heals) => SetHealth(heals);
    public void restoreSomeHeals(int amount) => RestoreHealth(amount);
    public void restoreDefaultHeals() => RestoreFullHealth();
    public int getHeals() => CurrentHealth;
    public void receiveDamage(int damageAmount) => ReceiveDamage(damageAmount);
    public bool getIsAlive() => IsAlive;

    private void OnValidate()
    {
        defaultHeals = Mathf.Max(1, defaultHeals);
        currentHeals = Mathf.Clamp(currentHeals, 0, defaultHeals);
    }
}
