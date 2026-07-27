using UnityEngine;
using ZoneUA.Combat;
using ZoneUA.Factions;

public sealed class Bullet : MonoBehaviour
{
    [Header("Definition")]
    [SerializeField, Tooltip("Preferred projectile configuration. Legacy fields are used when empty.")]
    private ProjectileDefinition projectileDefinition;

    [Header("Legacy Damage Fallback")]
    [SerializeField, Min(0)] private int minDamage = 1;
    [SerializeField, Min(0)] private int maxDamage = 1;
    [SerializeField, Min(1f)] private float criticalScale = 1.5f;
    [SerializeField, Range(0, 100)] private int criticalChanse = 20;
    [SerializeField, Min(1)] private int criticalMaximum = 100;

    [Header("Legacy Presentation and Filtering")]
    [SerializeField] private Transform damageDealPrefab;
    [SerializeField] private string[] whoRecieveDamage = { "Player", "Enemy" };

    private ProjectileDefinition runtimeDefinition;
    private GameObject source;
    private GameObject instigator;
    private bool hasHit;

    private ProjectileDefinition EffectiveDefinition =>
        runtimeDefinition != null ? runtimeDefinition : projectileDefinition;

    private void OnEnable()
    {
        hasHit = false;
    }

    private void OnDisable()
    {
        runtimeDefinition = null;
        source = null;
        instigator = null;
        hasHit = false;
    }

    public void Configure(
        ProjectileDefinition definition,
        GameObject projectileSource,
        GameObject projectileInstigator)
    {
        runtimeDefinition = definition;
        source = projectileSource;
        instigator = projectileInstigator;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit || !CanDamage(other))
        {
            return;
        }

        hasHit = true;

        Health health = other.GetComponentInParent<Health>();
        if (health != null && health.IsAlive)
        {
            DamageRoll damageRoll = RollDamage();
            DamageInfo damageInfo = new DamageInfo(
                damageRoll.Amount,
                source != null ? source : gameObject,
                instigator,
                other.ClosestPoint(transform.position),
                transform.right,
                EffectiveDefinition != null
                    ? EffectiveDefinition.DamageType
                    : DamageType.Bullet,
                damageRoll.IsCritical);

            health.ReceiveDamage(in damageInfo);
            ShowDamagePopup(other.transform.position, damageRoll);
        }

        ReleaseProjectile();
    }

    private DamageRoll RollDamage()
    {
        ProjectileDefinition definition = EffectiveDefinition;
        if (definition != null)
        {
            return ProjectileDamageCalculator.Roll(definition);
        }

        int damage = Random.Range(minDamage, maxDamage + 1);
        int criticalRoll = Random.Range(0, Mathf.Max(1, criticalMaximum));
        bool isCritical = criticalRoll < criticalChanse;

        if (isCritical)
        {
            damage = Mathf.RoundToInt(damage * criticalScale);
        }

        return new DamageRoll(damage, isCritical);
    }

    private bool CanDamage(Collider2D other)
    {
        FactionMember targetFaction = other.GetComponentInParent<FactionMember>();
        FactionMember sourceFaction = ResolveSourceFaction();

        if (sourceFaction != null && targetFaction != null)
        {
            return sourceFaction.CanDamage(targetFaction);
        }

        return MatchesLegacyDamageTag(other);
    }

    private FactionMember ResolveSourceFaction()
    {
        if (instigator != null)
        {
            FactionMember member = instigator.GetComponentInParent<FactionMember>();
            if (member != null)
            {
                return member;
            }
        }

        return source != null ? source.GetComponentInParent<FactionMember>() : null;
    }

    private bool MatchesLegacyDamageTag(Collider2D other)
    {
        if (whoRecieveDamage == null)
        {
            return false;
        }

        for (int i = 0; i < whoRecieveDamage.Length; i++)
        {
            string targetTag = whoRecieveDamage[i];
            if (!string.IsNullOrEmpty(targetTag) && other.CompareTag(targetTag))
            {
                return true;
            }
        }

        return false;
    }

    private void ShowDamagePopup(Vector3 position, DamageRoll damageRoll)
    {
        if (damageDealPrefab == null)
        {
            return;
        }

        DamageDealPopup.Create(
            damageDealPrefab,
            position,
            damageRoll.Amount,
            damageRoll.IsCritical);
    }

    private void ReleaseProjectile()
    {
        GlobalSystem system = GlobalSystem.Instance;
        if (system != null)
        {
            system.Release(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnValidate()
    {
        minDamage = Mathf.Max(0, minDamage);
        maxDamage = Mathf.Max(minDamage, maxDamage);
        criticalMaximum = Mathf.Max(1, criticalMaximum);
        criticalChanse = Mathf.Clamp(criticalChanse, 0, criticalMaximum);
        criticalScale = Mathf.Max(1f, criticalScale);
    }
}
