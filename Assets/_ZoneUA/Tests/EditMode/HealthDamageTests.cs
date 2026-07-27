using NUnit.Framework;
using ZoneUA.Factions;

namespace ZoneUA.Combat.Tests
{
    public sealed class HealthDamageTests
    {
        [Test]
        public void DamageResolver_AppliesResistanceAndRoundsUp()
        {
            DamageResolution result = DamageResolver.Resolve(25f, 0.2f);

            Assert.That(result.AppliedAmount, Is.EqualTo(20));
            Assert.That(result.Resistance, Is.EqualTo(0.2f));
        }

        [Test]
        public void DamageResolver_NegativeResistanceIncreasesDamage()
        {
            DamageResolution result = DamageResolver.Resolve(10f, -0.5f);

            Assert.That(result.AppliedAmount, Is.EqualTo(15));
        }

        [Test]
        public void DamageResolver_FullResistanceBlocksDamage()
        {
            DamageResolution result = DamageResolver.Resolve(10f, 1f);

            Assert.That(result.AppliedAmount, Is.Zero);
            Assert.That(result.WasBlocked, Is.True);
        }

        [Test]
        public void HealthState_DamageCannotDropBelowZero()
        {
            var state = new HealthState(100, 30);

            int applied = state.ApplyDamage(50);

            Assert.That(applied, Is.EqualTo(30));
            Assert.That(state.CurrentHealth, Is.Zero);
            Assert.That(state.IsAlive, Is.False);
        }

        [Test]
        public void HealthState_DeadStateRejectsFurtherDamageAndHealing()
        {
            var state = new HealthState(100, 10);
            state.ApplyDamage(10);

            Assert.That(state.ApplyDamage(5), Is.Zero);
            Assert.That(state.Heal(20), Is.Zero);
            Assert.That(state.CurrentHealth, Is.Zero);
        }

        [Test]
        public void HealthState_HealingIsClampedToMaximum()
        {
            var state = new HealthState(100, 80);

            int restored = state.Heal(50);

            Assert.That(restored, Is.EqualTo(20));
            Assert.That(state.CurrentHealth, Is.EqualTo(100));
        }

        [Test]
        public void HealthState_MaximumHealthCanPreserveRatio()
        {
            var state = new HealthState(100, 50);

            state.SetMaximumHealth(200, preserveRatio: true);

            Assert.That(state.MaximumHealth, Is.EqualTo(200));
            Assert.That(state.CurrentHealth, Is.EqualTo(100));
        }

        [Test]
        public void DeathState_CanOnlyBeEnteredOnce()
        {
            var state = new DeathState();

            Assert.That(state.TryEnter(), Is.True);
            Assert.That(state.TryEnter(), Is.False);
            Assert.That(state.IsDead, Is.True);
        }

        [Test]
        public void FactionPolicy_BlocksFriendlyFireByDefault()
        {
            bool canDamage = FactionDamagePolicy.CanDamage(
                sameFaction: true,
                allowFriendlyFire: false,
                relation: FactionRelation.Friendly);

            Assert.That(canDamage, Is.False);
        }

        [Test]
        public void FactionPolicy_AllowsConfiguredFriendlyFire()
        {
            bool canDamage = FactionDamagePolicy.CanDamage(
                sameFaction: true,
                allowFriendlyFire: true,
                relation: FactionRelation.Friendly);

            Assert.That(canDamage, Is.True);
        }

        [Test]
        public void FactionPolicy_OnlyAllowsHostileForeignFaction()
        {
            Assert.That(FactionDamagePolicy.CanDamage(false, false, FactionRelation.Hostile), Is.True);
            Assert.That(FactionDamagePolicy.CanDamage(false, false, FactionRelation.Neutral), Is.False);
            Assert.That(FactionDamagePolicy.CanDamage(false, false, FactionRelation.Friendly), Is.False);
        }
    }
}
