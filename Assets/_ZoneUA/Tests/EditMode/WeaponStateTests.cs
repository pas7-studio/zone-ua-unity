using NUnit.Framework;

namespace ZoneUA.Combat.Tests
{
    public sealed class WeaponStateTests
    {
        [Test]
        public void SingleFire_ConsumesOnlyOneShotPerTriggerPress()
        {
            var state = new WeaponFireState();
            state.StartTrigger();

            Assert.That(state.ShouldAttemptShot(WeaponFireMode.Single, 0f), Is.True);
            state.RegisterSuccessfulShot(WeaponFireMode.Single, 0f, 0.2f, 0f);

            Assert.That(state.ShouldAttemptShot(WeaponFireMode.Single, 0.2f), Is.False);

            state.StopTrigger();
            state.StartTrigger();

            Assert.That(state.ShouldAttemptShot(WeaponFireMode.Single, 0.2f), Is.True);
        }

        [Test]
        public void RepeatedStartTrigger_DoesNotRearmSingleFire()
        {
            var state = new WeaponFireState();
            state.StartTrigger();
            state.RegisterSuccessfulShot(WeaponFireMode.Single, 0f, 0.2f, 0f);

            state.StartTrigger();

            Assert.That(state.ShouldAttemptShot(WeaponFireMode.Single, 0.2f), Is.False);
        }

        [Test]
        public void AutomaticFire_RespectsShotInterval()
        {
            var state = new WeaponFireState();
            state.StartTrigger();

            Assert.That(state.ShouldAttemptShot(WeaponFireMode.Automatic, 1f), Is.True);
            state.RegisterSuccessfulShot(WeaponFireMode.Automatic, 1f, 0.25f, 0f);

            Assert.That(state.ShouldAttemptShot(WeaponFireMode.Automatic, 1.24f), Is.False);
            Assert.That(state.ShouldAttemptShot(WeaponFireMode.Automatic, 1.25f), Is.True);
        }

        [Test]
        public void BurstFire_StopsAfterConfiguredShotCount()
        {
            var state = new WeaponFireState();

            Assert.That(state.TryStartBurst(3, 0f), Is.True);

            for (int shot = 0; shot < 3; shot++)
            {
                float time = shot * 0.1f;
                Assert.That(state.ShouldAttemptShot(WeaponFireMode.Burst, time), Is.True);
                state.RegisterSuccessfulShot(WeaponFireMode.Burst, time, 0.1f, 0.5f);
            }

            Assert.That(state.IsBurstActive, Is.False);
            Assert.That(state.BurstShotsRemaining, Is.Zero);
        }

        [Test]
        public void BurstFire_RespectsCooldownBeforeRestart()
        {
            var state = new WeaponFireState();
            state.TryStartBurst(1, 2f);
            state.RegisterSuccessfulShot(WeaponFireMode.Burst, 2f, 0.1f, 0.5f);

            Assert.That(state.TryStartBurst(1, 2.49f), Is.False);
            Assert.That(state.TryStartBurst(1, 2.5f), Is.True);
        }

        [Test]
        public void FailedShot_CancelsActiveBurst()
        {
            var state = new WeaponFireState();
            state.TryStartBurst(3, 0f);

            state.RegisterFailedShot();

            Assert.That(state.IsBurstActive, Is.False);
            Assert.That(state.BurstShotsRemaining, Is.Zero);
        }

        [Test]
        public void Reload_CompletesOnlyAfterConfiguredDuration()
        {
            var state = new WeaponReloadState();

            Assert.That(state.TryStart(2, 10, 5f, 1.5f), Is.True);
            Assert.That(state.TryComplete(6.49f), Is.False);
            Assert.That(state.IsReloading, Is.True);
            Assert.That(state.TryComplete(6.5f), Is.True);
            Assert.That(state.IsReloading, Is.False);
        }

        [Test]
        public void Reload_DoesNotStartForFullMagazineOrWhileAlreadyReloading()
        {
            var state = new WeaponReloadState();

            Assert.That(state.TryStart(10, 10, 0f, 1f), Is.False);
            Assert.That(state.TryStart(2, 10, 0f, 1f), Is.True);
            Assert.That(state.TryStart(1, 10, 0.1f, 1f), Is.False);
        }

        [Test]
        public void Reset_ClearsFireAndReloadTransientState()
        {
            var fireState = new WeaponFireState();
            var reloadState = new WeaponReloadState();

            fireState.StartTrigger();
            fireState.TryStartBurst(3, 0f);
            reloadState.TryStart(1, 10, 0f, 2f);

            fireState.Reset();
            reloadState.Reset();

            Assert.That(fireState.TriggerHeld, Is.False);
            Assert.That(fireState.IsBurstActive, Is.False);
            Assert.That(fireState.NextShotTime, Is.Zero);
            Assert.That(reloadState.IsReloading, Is.False);
            Assert.That(reloadState.CompletionTime, Is.Zero);
        }
    }
}
