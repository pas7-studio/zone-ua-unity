using NUnit.Framework;
using UnityEngine;
using ZoneUA.World;

namespace ZoneUA.Combat.Tests
{
    public sealed class WorldGenerationTests
    {
        [Test]
        public void GenerationContext_SameSeedProducesSameOffsets()
        {
            var first = new WorldGenerationContext(42);
            var second = new WorldGenerationContext(42);

            Assert.That(second.ElevationOffset, Is.EqualTo(first.ElevationOffset));
            Assert.That(second.MoistureOffset, Is.EqualTo(first.MoistureOffset));
            Assert.That(second.TemperatureOffset, Is.EqualTo(first.TemperatureOffset));
            Assert.That(second.VegetationOffset, Is.EqualTo(first.VegetationOffset));
            Assert.That(second.SettlementOffset, Is.EqualTo(first.SettlementOffset));
        }

        [Test]
        public void GenerationContext_DifferentSeedsProduceDifferentOffsets()
        {
            var first = new WorldGenerationContext(1);
            var second = new WorldGenerationContext(2);

            Assert.That(second.ElevationOffset, Is.Not.EqualTo(first.ElevationOffset));
        }

        [Test]
        public void DeterministicValue_IsStableAndNormalised()
        {
            float first = WorldDeterminism.Value01(100, 4, 8, 12);
            float second = WorldDeterminism.Value01(100, 4, 8, 12);

            Assert.That(second, Is.EqualTo(first));
            Assert.That(first, Is.InRange(0f, 1f));
        }

        [Test]
        public void DeterministicIndex_StaysInsideCollectionBounds()
        {
            for (int x = -20; x <= 20; x++)
            {
                int index = WorldDeterminism.Index(55, x, x * 2, 7, 9);
                Assert.That(index, Is.InRange(0, 6));
            }
        }

        [Test]
        public void NoiseSampler_SameSettingsSeedAndCoordinateProduceSameSample()
        {
            WorldGenerationSettings settings = ScriptableObject.CreateInstance<WorldGenerationSettings>();
            try
            {
                var context = new WorldGenerationContext(777);
                WorldSample first = WorldNoiseSampler.Sample(settings, in context, 10, -3);
                WorldSample second = WorldNoiseSampler.Sample(settings, in context, 10, -3);

                Assert.That(second.Elevation, Is.EqualTo(first.Elevation));
                Assert.That(second.Moisture, Is.EqualTo(first.Moisture));
                Assert.That(second.Temperature, Is.EqualTo(first.Temperature));
                Assert.That(second.Vegetation, Is.EqualTo(first.Vegetation));
                Assert.That(second.Settlement, Is.EqualTo(first.Settlement));
            }
            finally
            {
                Object.DestroyImmediate(settings);
            }
        }

        [Test]
        public void FixedSeed_IgnoresRuntimeSeed()
        {
            WorldGenerationSettings settings = ScriptableObject.CreateInstance<WorldGenerationSettings>();
            try
            {
                Assert.That(settings.ResolveSeed(987654), Is.EqualTo(settings.Seed));
            }
            finally
            {
                Object.DestroyImmediate(settings);
            }
        }
    }
}
