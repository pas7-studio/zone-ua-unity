using System;
using UnityEngine;
using ZoneUA.Combat;

[RequireComponent(typeof(Death))]
public sealed class Health : MonoBehaviour, IDamageable
{
    [Header("Health Configuration")]
    [SerializeField, Min(1), Tooltip("Maximum health for this character.")]
    private int defaultHeals = 100;

    [SerializeField, Tooltip("Ignores incoming damage while enabled.")]
    private bool isImunable;

    [SerializeField, Tooltip("Optional per-damage-type resistance profile.")]
    private DamageResistanceProfile resistanceProfile;

    [SerializeField, Min(0f), Tooltip("Final multiplier applied after resistance.")]
    private float incomingDamageMultiplier = 1f;

    [Header("Presentation")]
    [SerializeField, Tooltip("Optional presenter for blood, particles and damage popups.")]
    private DamageEffectsPresenter damageEffectsPresenter;

    [Header("Runtime State (Read Only)")]
    [SerializeField, HideInInspector] private int currentHeals = 100;

    private Death death;
    private HealthState state;
    private bool deathRaised;

    public event Action<DamageInfo> Damaged;
    public event Action<DamageInfo, DamageResolution> DamageResolved;
    public event Action<int, int> HealthChanged;
    public event Action<int> Healed;
    public event Action Died;

    public int CurrentHealth => state != null ? state.CurrentHealth : currentHeals;
    public int MaximumHealth => state != null ? state.MaximumHealth : defaultHeals;
    public bool IsAlive => state != null ? state.IsAlive : currentHeals > 0;
    public bool IsImmune => isImunable;

    private void Awake()
    {
        death = GetComponent<Death>();
        damageEffectsPresenter ??= GetComponent<DamageEffectsPresenter>();
        state = new HealthState(defaultHeals, currentHeals);
        SyncSerializedState();
    }

    private void Start()
    {
        if (!state.IsAlive)
        {
            DieOnce();
        }
        else
        {
            HealthChanged?.Invoke(state.CurrentHealth, state.MaximumHealth);
        }
    }

    public void SetHealth(int health)
    {
        if (deathRaised) return;

        int previous = state.CurrentHealth;
        state.SetHealth(health);
        SyncSerializedState();
        RaiseHealthChanged(previous);

        if (!state.IsAlive) DieOnce();
    }

    public void SetMaximumHealth(int maximumHealth, bool preserveRatio = false)
    {
        if (deathRaised) return;

        int previous = state.CurrentHealth;
        state.SetMaximumHealth(maximumHealth, preserveRatio);
        defaultHeals = state.MaximumHealth;
        SyncSerializedState();
        RaiseHealthChanged(previous, force: true);

        if (!state.IsAlive) DieOnce();
    }

    public void RestoreHealth(int amount)
    {
        int restored = state.Heal(amount);
        if (restored <= 0) return;

        int previous = state.CurrentHealth - restored;
        SyncSerializedState();
        Healed?.Invoke(restored);
        RaiseHealthChanged(previous);
    }

    public void RestoreFullHealth() => RestoreHealth(state.MaximumHealth - state.CurrentHealth);

    public void ReceiveDamage(in DamageInfo damageInfo)
    {
        if (!state.IsAlive || isImunable || damageInfo.Amount <= 0f) return;

        float resistance = resistanceProfile != null
            ? resistanceProfile.GetResistance(damageInfo.Type)
            : 0f;
        DamageResolution resolution = DamageResolver.Resolve(
            damageInfo.Amount,
            resistance,
            incomingDamageMultiplier);

        DamageResolved?.Invoke(damageInfo, resolution);
        if (resolution.AppliedAmount <= 0) return;

        int previous = state.CurrentHealth;
        int applied = state.ApplyDamage(resolution.AppliedAmount);
        if (applied <= 0) return;

        SyncSerializedState();
        Damaged?.Invoke(damageInfo);
        RaiseHealthChanged(previous);
        damageEffectsPresenter?.Present(in damageInfo, applied);

        if (!state.IsAlive) DieOnce();
    }

    [Obsolete("Use ReceiveDamage(in DamageInfo) so source, type, position and impulse are preserved.")]
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

    private void DieOnce()
    {
        if (deathRaised) return;

        deathRaised = true;
        state.SetHealth(0);
        SyncSerializedState();
        death?.Dead();
        Died?.Invoke();
    }

    private void RaiseHealthChanged(int previousHealth, bool force = false)
    {
        if (force || previousHealth != state.CurrentHealth)
        {
            HealthChanged?.Invoke(state.CurrentHealth, state.MaximumHealth);
        }
    }

    private void SyncSerializedState() => currentHeals = state.CurrentHealth;

    [Obsolete("Health state is evaluated immediately. Subscribe to Died or use IsAlive instead.")]
    public void HealthLogic()
    {
        if (!state.IsAlive) DieOnce();
    }

    // Compatibility API retained only for serialized UnityEvents during prefab migration.
    [Obsolete("Use SetHealth(int).")]
    public void setHeals(int heals) => SetHealth(heals);

    [Obsolete("Use RestoreHealth(int).")]
    public void restoreSomeHeals(int amount) => RestoreHealth(amount);

    [Obsolete("Use RestoreFullHealth().")]
    public void restoreDefaultHeals() => RestoreFullHealth();

    [Obsolete("Use CurrentHealth.")]
    public int getHeals() => CurrentHealth;

    [Obsolete("Use ReceiveDamage(in DamageInfo).")]
    public void receiveDamage(int damageAmount) => ReceiveDamage(damageAmount);

    [Obsolete("Use IsAlive.")]
    public bool getIsAlive() => IsAlive;

    private void OnValidate()
    {
        defaultHeals = Mathf.Max(1, defaultHeals);
        currentHeals = Mathf.Clamp(currentHeals, 0, defaultHeals);
        incomingDamageMultiplier = Mathf.Max(0f, incomingDamageMultiplier);
    }
}
