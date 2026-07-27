using System.Collections.Generic;
using UnityEngine;

namespace ZoneUA.World
{
    [CreateAssetMenu(fileName = "WorldGenerationSettings", menuName = "Zone UA/World/Generation Settings")]
    public sealed class WorldGenerationSettings : ScriptableObject
    {
        [Header("Seed")]
        [SerializeField] private int seed = 12345;
        [SerializeField, Tooltip("Use the configured seed instead of a runtime-generated seed.")]
        private bool useFixedSeed = true;

        [Header("Grid")]
        [SerializeField, Min(1)] private int mapWidth = 64;
        [SerializeField, Min(1)] private int mapHeight = 64;
        [SerializeField, Min(0.01f)] private float tileSize = 1f;

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
        [SerializeField, Min(0f)] private float decorationDensityMultiplier = 1f;
        [SerializeField] private BiomeDefinition fallbackBiome;
        [SerializeField] private BiomeDefinition[] biomes;

        public int Seed => seed;
        public bool UseFixedSeed => useFixedSeed;
        public int MapWidth => mapWidth;
        public int MapHeight => mapHeight;
        public float TileSize => tileSize;
        public int ChunkSize => chunkSize;
        public int ActiveChunkRadius => activeChunkRadius;
        public int GenerationBudgetPerFrame => generationBudgetPerFrame;
        public float ElevationScale => elevationScale;
        public float MoistureScale => moistureScale;
        public float TemperatureScale => temperatureScale;
        public float VegetationScale => vegetationScale;
        public float SettlementScale => settlementScale;
        public float MinimumDecorationDistance => minimumDecorationDistance;
        public float DecorationDensityMultiplier => decorationDensityMultiplier;
        public BiomeDefinition FallbackBiome => fallbackBiome;
        public BiomeDefinition[] Biomes => biomes;

        public int ResolveSeed(int runtimeSeed) => useFixedSeed ? seed : runtimeSeed;

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

        public void CollectValidationErrors(List<string> errors)
        {
            if (errors == null)
            {
                return;
            }

            if (fallbackBiome == null)
            {
                errors.Add("Fallback biome is not assigned.");
            }

            if (biomes == null || biomes.Length == 0)
            {
                errors.Add("No biome definitions are assigned.");
                return;
            }

            var ids = new HashSet<string>();
            for (int i = 0; i < biomes.Length; i++)
            {
                BiomeDefinition biome = biomes[i];
                if (biome == null)
                {
                    errors.Add($"Biome slot {i} is empty.");
                    continue;
                }

                if (!ids.Add(biome.Id))
                {
                    errors.Add($"Biome id '{biome.Id}' is duplicated.");
                }

                if (biome.TerrainPrefab == null)
                {
                    errors.Add($"Biome '{biome.DisplayName}' has no terrain prefab.");
                }
            }
        }

        private void OnValidate()
        {
            mapWidth = Mathf.Max(1, mapWidth);
            mapHeight = Mathf.Max(1, mapHeight);
            tileSize = Mathf.Max(0.01f, tileSize);
            chunkSize = Mathf.Max(1, chunkSize);
            activeChunkRadius = Mathf.Max(1, activeChunkRadius);
            generationBudgetPerFrame = Mathf.Max(1, generationBudgetPerFrame);
            elevationScale = Mathf.Max(0.0001f, elevationScale);
            moistureScale = Mathf.Max(0.0001f, moistureScale);
            temperatureScale = Mathf.Max(0.0001f, temperatureScale);
            vegetationScale = Mathf.Max(0.0001f, vegetationScale);
            settlementScale = Mathf.Max(0.0001f, settlementScale);
            minimumDecorationDistance = Mathf.Max(0f, minimumDecorationDistance);
            decorationDensityMultiplier = Mathf.Max(0f, decorationDensityMultiplier);
        }
    }
}
