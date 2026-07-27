using System;
using UnityEngine;

namespace ZoneUA.World
{
    public readonly struct WorldGenerationContext
    {
        public WorldGenerationContext(int seed)
        {
            Seed = seed;
            var random = new System.Random(seed);
            ElevationOffset = NextOffset(random);
            MoistureOffset = NextOffset(random);
            TemperatureOffset = NextOffset(random);
            VegetationOffset = NextOffset(random);
            SettlementOffset = NextOffset(random);
        }

        public int Seed { get; }
        public Vector2 ElevationOffset { get; }
        public Vector2 MoistureOffset { get; }
        public Vector2 TemperatureOffset { get; }
        public Vector2 VegetationOffset { get; }
        public Vector2 SettlementOffset { get; }

        private static Vector2 NextOffset(System.Random random)
        {
            return new Vector2(
                Mathf.Lerp(-10000f, 10000f, (float)random.NextDouble()),
                Mathf.Lerp(-10000f, 10000f, (float)random.NextDouble()));
        }
    }
}
