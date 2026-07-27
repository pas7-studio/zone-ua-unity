using UnityEngine;

namespace ZoneUA.World
{
    [CreateAssetMenu(fileName = "Biome", menuName = "Zone UA/World/Biome Definition")]
    public sealed class BiomeDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id = "biome";
        [SerializeField] private string displayName = "Biome";

        [Header("Climate Range")]
        [SerializeField, Range(0f, 1f)] private float minimumElevation;
        [SerializeField, Range(0f, 1f)] private float maximumElevation = 1f;
        [SerializeField, Range(0f, 1f)] private float minimumMoisture;
        [SerializeField, Range(0f, 1f)] private float maximumMoisture = 1f;
        [SerializeField, Range(0f, 1f)] private float minimumTemperature;
        [SerializeField, Range(0f, 1f)] private float maximumTemperature = 1f;

        [Header("Presentation")]
        [SerializeField, Tooltip("Base tile or chunk-view prefab used for this biome.")]
        private GameObject terrainPrefab;
        [SerializeField, Tooltip("Optional decoration prefabs sampled deterministically by the world generator.")]
        private GameObject[] decorationPrefabs;
        [SerializeField, Min(0f)] private float decorationDensity = 0.1f;

        public string Id => id;
        public string DisplayName => displayName;
        public float MinimumElevation => minimumElevation;
        public float MaximumElevation => maximumElevation;
        public float MinimumMoisture => minimumMoisture;
        public float MaximumMoisture => maximumMoisture;
        public float MinimumTemperature => minimumTemperature;
        public float MaximumTemperature => maximumTemperature;
        public GameObject TerrainPrefab => terrainPrefab;
        public GameObject[] DecorationPrefabs => decorationPrefabs;
        public float DecorationDensity => decorationDensity;

        public bool Matches(float elevation, float moisture, float temperature)
        {
            return elevation >= minimumElevation && elevation <= maximumElevation
                && moisture >= minimumMoisture && moisture <= maximumMoisture
                && temperature >= minimumTemperature && temperature <= maximumTemperature;
        }

        private void OnValidate()
        {
            id = string.IsNullOrWhiteSpace(id) ? name.Trim().ToLowerInvariant().Replace(' ', '-') : id.Trim();
            displayName = string.IsNullOrWhiteSpace(displayName) ? name : displayName.Trim();
            maximumElevation = Mathf.Max(minimumElevation, maximumElevation);
            maximumMoisture = Mathf.Max(minimumMoisture, maximumMoisture);
            maximumTemperature = Mathf.Max(minimumTemperature, maximumTemperature);
            decorationDensity = Mathf.Max(0f, decorationDensity);
        }
    }
}
