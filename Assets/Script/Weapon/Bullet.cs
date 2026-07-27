using UnityEngine;

public sealed class Bullet : MonoBehaviour
{
    [SerializeField, Min(0)] private int minDamage = 1;
    [SerializeField, Min(0)] private int maxDamage = 1;
    [SerializeField, Min(1f)] private float criticalScale = 1.5f;
    [SerializeField, Range(0, 100)] private int criticalChanse = 20;
    [SerializeField, Min(1)] private int criticalMaximum = 100;
    [SerializeField] private Transform damageDealPrefab;
    [SerializeField] private string[] whoRecieveDamage = { "Player", "Enemy" };

    private bool hasHit;

    private void OnEnable()
    {
        hasHit = false;
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
            int damage = Random.Range(minDamage, maxDamage + 1);
            int criticalRoll = Random.Range(0, Mathf.Max(1, criticalMaximum));
            bool isCritical = criticalRoll < criticalChanse;

            if (isCritical)
            {
                damage = Mathf.RoundToInt(damage * criticalScale);
            }

            health.ReceiveDamage(damage);

            if (damageDealPrefab != null)
            {
                DamageDealPopup.Create(
                    damageDealPrefab,
                    other.transform.position,
                    damage,
                    isCritical);
            }
        }

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

    private bool CanDamage(Collider2D other)
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

    private void OnValidate()
    {
        minDamage = Mathf.Max(0, minDamage);
        maxDamage = Mathf.Max(minDamage, maxDamage);
        criticalMaximum = Mathf.Max(1, criticalMaximum);
        criticalChanse = Mathf.Clamp(criticalChanse, 0, criticalMaximum);
    }
}
