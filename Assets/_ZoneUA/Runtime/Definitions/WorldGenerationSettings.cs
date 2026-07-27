using UnityEngine;

namespace ZoneUA.World
{
    [CreateAssetMenu(fileName = "WorldGenerationSettings", menuName = "Zone UA/World/Generation Settings")]
    public sealed class WorldGenerationSettings : ScriptableObject
    {
        [Header("Seed")]
        [SerializeField] private int seed = 12345;
        [SerializeField, Tooltip("Use the configured seed instead of generating one at runtime.")]
        private bool useFixedSeed = true;

        [Header("Chunks")]
        [SerializeField, Min(1)] private int chunkSize = 32;
        [SerializeField, Min(1)] private int activeChunkRadius = 2;
        [SerializeField, Min(1)] private int generationBudgetPerFrame = 1;

        [Header("Noise")]
        [SerializeField, Min(0.0001f)] private float elevationScale = 0.02f;
        [SerializeField, Min(0.0001f)] private float moistureScale = 0.018f;
        [SerializeField, Min(0.0001f)] private float temperatureScale = 0.012f;
        [SerializeField, Min(0.0001f)] private float vegetationScale = 0.04f;
        [SerializeField, Min(0.0001f)] private float settlementScale = 0.008f;

        [Header("Placement")]
        [SerializeField, Min(0f)] private float minimumDecorationDistance = 0.5f;
        [SerializeField] private BiomeDefinition fallbackBiome;
        [SerializeField] private BiomeDefinition[] biomes;

        public int Seed => seed;
        public bool UseFixedSeed => useFixedSeed;
        public int ChunkSize => chunkSize;
        public int ActiveChunkRadius => activeChunkRadius;
        public int GenerationBudgetPerFrame => generationBudgetPerFrame;
        public float ElevationScale => elevationScale;
        public float MoistureScale => moistureScale;
        public float TemperatureScale => temperatureScale;
        public float VegetationScale => vegetationScale;
        public float SettlementScale => settlementScale;
        public float MinimumDecorationDistance => minimumDecorationDistance;
        public BiomeDefinition FallbackBiome => fallbackBiome;
        public BiomeDefinition[] Biomes => biomes;

        public BiomeDefinition ResolveBiome(float elevation, float moisture, float temperature)
        {
            if (biomes != null)
            {
                for (int i = 0; i < biomes.Length; i++)
                {
                    BiomeDefinition biome = biomes[i];
                    if (biome != null && biome.Matches(elevation, moisture, temperature))
                    {
                        return biome;
                    }
                }
            }

            return fallbackBiome;
        }

        private void OnValidate()
        {
            chunkSize = Mathf.Max(1, chunkSize);
            activeChunkRadius = Mathf.Max(1, activeChunkRadius);
            generationBudgetPerFrame = Mathf.Max(1, generationBudgetPerFrame);
            elevationScale = Mathf.Max(0.0001f, elevationScale);
            moistureScale = Mathf.Max(0.0001f, moistureScale);
            temperatureScale = Mathf.Max(0.0001f, temperatureScale);
            vegetationScale = Mathf.Max(0.0001f, vegetationScale);
            settlementScale = Mathf.Max(0.0001f, settlementScale);
            minimumDecorationDistance = Mathf.Max(0f, minimumDecorationDistance);
        }
    }
}
