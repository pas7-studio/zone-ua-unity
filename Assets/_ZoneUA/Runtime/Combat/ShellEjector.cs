using UnityEngine;

namespace ZoneUA.Combat
{
    [DisallowMultipleComponent]
    public sealed class ShellEjector : MonoBehaviour
    {
        [SerializeField] private Transform ejectionPoint;
        [SerializeField] private GameObject shellPrefab;
        [SerializeField, Min(0f)] private float impulse = 10f;
        [SerializeField, Min(0f)] private float lateralOffset = 0.1f;
        [SerializeField, Range(0f, 180f)] private float rotationRange = 45f;
        [SerializeField, Min(0f)] private float lifetime = 5f;
        [SerializeField, Tooltip("Optional component implementing IRuntimeObjectSpawner.")]
        private MonoBehaviour spawnerSource;

        private IRuntimeObjectSpawner spawner;

        private void Awake()
        {
            ResolveSpawner();
        }

        public GameObject Eject()
        {
            if (shellPrefab == null || ejectionPoint == null)
            {
                return null;
            }

            ResolveSpawner();
            Vector3 position = ejectionPoint.position + ejectionPoint.up * Random.Range(-lateralOffset, lateralOffset);
            Quaternion rotation = Quaternion.Euler(0f, 0f, Random.Range(-rotationRange, rotationRange));
            GameObject instance = spawner != null
                ? spawner.Spawn(shellPrefab, position, rotation)
                : Instantiate(shellPrefab, position, rotation);

            if (instance != null && instance.TryGetComponent(out Rigidbody2D body))
            {
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
                body.AddForce(-ejectionPoint.up * impulse, ForceMode2D.Impulse);
            }

            if (instance != null && lifetime > 0f)
            {
                if (spawner != null)
                {
                    spawner.ReleaseAfter(instance, lifetime);
                }
                else
                {
                    Destroy(instance, lifetime);
                }
            }

            return instance;
        }

        public void Configure(Transform point, GameObject prefab)
        {
            ejectionPoint = point;
            shellPrefab = prefab;
        }

        private void ResolveSpawner()
        {
            spawner = spawnerSource as IRuntimeObjectSpawner;
        }

        private void OnValidate()
        {
            impulse = Mathf.Max(0f, impulse);
            lateralOffset = Mathf.Max(0f, lateralOffset);
            lifetime = Mathf.Max(0f, lifetime);
        }
    }
}
