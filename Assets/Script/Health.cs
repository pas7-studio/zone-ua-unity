using Assets.Script;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Death))]
public sealed class Health : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int currentHeals = 100;
    [SerializeField, Min(1)] private int defaultHeals = 100;
    [SerializeField] private bool isImunable;
    [SerializeField] private bool isAlive = true;

    private Death death;
    private GlobalSystem globalSystem;

    public int CurrentHealth => currentHeals;
    public int MaximumHealth => defaultHeals;
    public bool IsAlive => isAlive;

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

        currentHeals = Mathf.Clamp(health, 0, defaultHeals);
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

        currentHeals = Mathf.Clamp(currentHeals + amount, 0, defaultHeals);
    }

    public void RestoreFullHealth()
    {
        if (isAlive)
        {
            currentHeals = defaultHeals;
        }
    }

    public void ReceiveDamage(int damageAmount)
    {
        if (!isAlive || isImunable || damageAmount <= 0)
        {
            return;
        }

        currentHeals = Mathf.Max(0, currentHeals - damageAmount);
        SpawnBlood();

        if (currentHeals == 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (death.IsDead)
        {
            return;
        }

        isAlive = false;
        death.Dead();
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
                Random.insideUnitCircle * globalSystem.BloodSpawnRadius;

            Quaternion rotation = Quaternion.Euler(0f, 0f, Random.Range(-180f, 180f));
            GameObject bloodDrop = Instantiate(
                bloodPrefab,
                spawnPosition,
                rotation,
                globalSystem.RuntimeContainer);

            if (!bloodDrop.TryGetComponent(out Rigidbody2D body))
            {
                continue;
            }

            Vector2 forceDirection = Random.insideUnitCircle.normalized * globalSystem.BloodImpulseSpeed;
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

        ParticleSystem instance = Instantiate(
            bloodEffectPrefab,
            transform.position,
            bloodEffectPrefab.transform.rotation,
            globalSystem.RuntimeContainer);

        instance.Play();

        ParticleSystem.MainModule main = instance.main;
        float lifetime = main.startLifetime.constantMax;
        Destroy(instance.gameObject, Mathf.Max(0.1f, lifetime));
    }

    // Backwards-compatible methods used by existing scripts or UnityEvents.
    public void HealthLogic()
    {
        if (currentHeals <= 0)
        {
            Die();
        }
    }

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
