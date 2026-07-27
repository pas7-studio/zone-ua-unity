using UnityEngine;

namespace ZoneUA.Combat
{
    [CreateAssetMenu(fileName = "DamageEffectSettings", menuName = "Zone UA/Combat/Damage Effect Settings")]
    public sealed class DamageEffectSettings : ScriptableObject
    {
        [Header("Decals")]
        [SerializeField, Min(0)] private int decalCount = 10;
        [SerializeField, Min(0f)] private float spawnRadius = 1f;
        [SerializeField, Min(0f)] private float impulseSpeed = 1f;
        [SerializeField, Min(0f)] private float impulseDuration = 1f;
        [SerializeField, Tooltip("Presentation prefabs selected by the effect presenter.")]
        private GameObject[] decalPrefabs;

        [Header("Particles")]
        [SerializeField] private ParticleSystem hitParticlePrefab;
        [SerializeField, Min(0f)] private float additionalParticleLifetime = 0.25f;

        [Header("Popup")]
        [SerializeField] private Transform damagePopupPrefab;
        [SerializeField, Min(0f)] private float popupLifetime = 1f;

        public int DecalCount => decalCount;
        public float SpawnRadius => spawnRadius;
        public float ImpulseSpeed => impulseSpeed;
        public float ImpulseDuration => impulseDuration;
        public GameObject[] DecalPrefabs => decalPrefabs;
        public ParticleSystem HitParticlePrefab => hitParticlePrefab;
        public float AdditionalParticleLifetime => additionalParticleLifetime;
        public Transform DamagePopupPrefab => damagePopupPrefab;
        public float PopupLifetime => popupLifetime;

        private void OnValidate()
        {
            decalCount = Mathf.Max(0, decalCount);
            spawnRadius = Mathf.Max(0f, spawnRadius);
            impulseSpeed = Mathf.Max(0f, impulseSpeed);
            impulseDuration = Mathf.Max(0f, impulseDuration);
            additionalParticleLifetime = Mathf.Max(0f, additionalParticleLifetime);
            popupLifetime = Mathf.Max(0f, popupLifetime);
        }
    }
}
