using UnityEngine;

namespace ZoneUA.World
{
    public readonly struct WorldSample
    {
        public WorldSample(float elevation, float moisture, float temperature, float vegetation, float settlement)
        {
            Elevation = Mathf.Clamp01(elevation);
            Moisture = Mathf.Clamp01(moisture);
            Temperature = Mathf.Clamp01(temperature);
            Vegetation = Mathf.Clamp01(vegetation);
            Settlement = Mathf.Clamp01(settlement);
        }

        public float Elevation { get; }
        public float Moisture { get; }
        public float Temperature { get; }
        public float Vegetation { get; }
        public float Settlement { get; }
    }

    public static class WorldNoiseSampler
    {
        public static WorldSample Sample(WorldGenerationSettings settings, in WorldGenerationContext context, int x, int y)
        {
            if (settings == null)
            {
                return default;
            }

            Vector2 coordinate = new Vector2(x, y);
            return new WorldSample(
                SampleChannel(coordinate, context.ElevationOffset, settings.ElevationScale),
                SampleChannel(coordinate, context.MoistureOffset, settings.MoistureScale),
                SampleChannel(coordinate, context.TemperatureOffset, settings.TemperatureScale),
                SampleChannel(coordinate, context.VegetationOffset, settings.VegetationScale),
                SampleChannel(coordinate, context.SettlementOffset, settings.SettlementScale));
        }

        private static float SampleChannel(Vector2 coordinate, Vector2 offset, float scale)
        {
            float safeScale = Mathf.Max(0.0001f, scale);
            return Mathf.PerlinNoise(
                coordinate.x * safeScale + offset.x,
                coordinate.y * safeScale + offset.y);
        }
    }
}
