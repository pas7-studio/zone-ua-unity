using UnityEngine;

namespace ZoneUA.Combat
{
    [CreateAssetMenu(fileName = "Projectile", menuName = "Zone UA/Combat/Projectile Definition")]
    public sealed class ProjectileDefinition : ScriptableObject
    {
        [Header("Prefab")]
        [SerializeField, Tooltip("Runtime projectile prefab. It should support pooling.")]
        private GameObject prefab;

        [Header("Damage")]
        [SerializeField, Min(0)] private int minimumDamage = 1;
        [SerializeField, Min(0)] private int maximumDamage = 1;
        [SerializeField] private DamageType damageType = DamageType.Bullet;
        [SerializeField, Range(0f, 1f)] private float criticalChance;
        [SerializeField, Min(1f)] private float criticalMultiplier = 1.5f;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float speed = 50f;
        [SerializeField, Min(0.01f)] private float lifetime = 5f;
        [SerializeField, Tooltip("Use continuous collision detection for high-speed Rigidbody2D projectiles.")]
        private bool continuousCollision = true;

        public GameObject Prefab => prefab;
        public int MinimumDamage => minimumDamage;
        public int MaximumDamage => maximumDamage;
        public DamageType DamageType => damageType;
        public float CriticalChance => criticalChance;
        public float CriticalMultiplier => criticalMultiplier;
        public float Speed => speed;
        public float Lifetime => lifetime;
        public bool ContinuousCollision => continuousCollision;

        private void OnValidate()
        {
            minimumDamage = Mathf.Max(0, minimumDamage);
            maximumDamage = Mathf.Max(minimumDamage, maximumDamage);
            speed = Mathf.Max(0f, speed);
            lifetime = Mathf.Max(0.01f, lifetime);
            criticalMultiplier = Mathf.Max(1f, criticalMultiplier);
        }
    }
}
