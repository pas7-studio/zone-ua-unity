using UnityEngine;

namespace ZoneUA.Combat
{
    [DisallowMultipleComponent]
    public sealed class ProjectileSpawner : MonoBehaviour
    {
        [SerializeField, Tooltip("Muzzle transform used as projectile origin.")]
        private Transform muzzle;

        [SerializeField, Tooltip("Optional component implementing IRuntimeObjectSpawner. Instantiate is used as fallback.")]
        private MonoBehaviour spawnerSource;

        private IRuntimeObjectSpawner spawner;

        public Transform Muzzle => muzzle;

        private void Awake()
        {
            ResolveSpawner();
        }

        public GameObject Spawn(ProjectileDefinition definition, GameObject fallbackPrefab, float fallbackSpeed)
        {
            GameObject prefab = definition != null && definition.Prefab != null
                ? definition.Prefab
                : fallbackPrefab;

            if (prefab == null || muzzle == null)
            {
                return null;
            }

            ResolveSpawner();
            GameObject instance = spawner != null
                ? spawner.Spawn(prefab, muzzle.position, muzzle.rotation)
                : Instantiate(prefab, muzzle.position, muzzle.rotation);

            if (instance != null && instance.TryGetComponent(out Rigidbody2D body))
            {
                float speed = definition != null ? definition.Speed : Mathf.Max(0f, fallbackSpeed);
                body.linearVelocity = muzzle.right * speed;

                if (definition != null && definition.ContinuousCollision)
                {
                    body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                }
            }

            return instance;
        }

        public void SetMuzzle(Transform value)
        {
            muzzle = value;
        }

        private void ResolveSpawner()
        {
            spawner = spawnerSource as IRuntimeObjectSpawner;
        }
    }
}
