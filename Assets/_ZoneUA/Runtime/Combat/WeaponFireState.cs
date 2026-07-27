using System;

namespace ZoneUA.Combat
{
    public sealed class WeaponFireState
    {
        private bool triggerHeld;
        private bool singleConsumed;
        private int burstShotsRemaining;
        private float nextShotTime;
        private float burstCooldownUntil;

        public bool TriggerHeld => triggerHeld;
        public bool IsBurstActive => burstShotsRemaining > 0;
        public int BurstShotsRemaining => burstShotsRemaining;
        public float NextShotTime => nextShotTime;
        public float BurstCooldownUntil => burstCooldownUntil;

        public void StartTrigger()
        {
            triggerHeld = true;
            singleConsumed = false;
        }

        public void StopTrigger()
        {
            triggerHeld = false;
            singleConsumed = false;
        }

        public bool CanStartBurst(float currentTime)
        {
            return burstShotsRemaining <= 0 && currentTime >= burstCooldownUntil;
        }

        public bool TryStartBurst(int burstSize, float currentTime)
        {
            if (!CanStartBurst(currentTime))
            {
                return false;
            }

            burstShotsRemaining = Math.Max(1, burstSize);
            nextShotTime = Math.Max(nextShotTime, currentTime);
            return true;
        }

        public bool ShouldAttemptShot(WeaponFireMode mode, float currentTime)
        {
            if (burstShotsRemaining > 0)
            {
                return currentTime >= nextShotTime;
            }

            if (!triggerHeld)
            {
                return false;
            }

            return mode switch
            {
                WeaponFireMode.Automatic => currentTime >= nextShotTime,
                WeaponFireMode.Single => !singleConsumed && currentTime >= nextShotTime,
                WeaponFireMode.Burst => false,
                _ => false
            };
        }

        public void RegisterSuccessfulShot(
            WeaponFireMode mode,
            float currentTime,
            float shotInterval,
            float burstCooldown)
        {
            nextShotTime = currentTime + Math.Max(0.001f, shotInterval);

            if (burstShotsRemaining > 0)
            {
                burstShotsRemaining--;
                if (burstShotsRemaining == 0)
                {
                    burstCooldownUntil = currentTime + Math.Max(0f, burstCooldown);
                }
            }

            if (mode == WeaponFireMode.Single)
            {
                singleConsumed = true;
            }
        }

        public void RegisterFailedShot(bool cancelBurst = true)
        {
            if (cancelBurst)
            {
                burstShotsRemaining = 0;
            }
        }

        public void CancelBurst()
        {
            burstShotsRemaining = 0;
        }

        public void Reset()
        {
            triggerHeld = false;
            singleConsumed = false;
            burstShotsRemaining = 0;
            nextShotTime = 0f;
            burstCooldownUntil = 0f;
        }
    }
}
