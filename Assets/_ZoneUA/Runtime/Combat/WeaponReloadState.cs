using System;

namespace ZoneUA.Combat
{
    public sealed class WeaponReloadState
    {
        private bool isReloading;
        private float completionTime;

        public bool IsReloading => isReloading;
        public float CompletionTime => completionTime;

        public bool TryStart(
            int currentAmmo,
            int magazineCapacity,
            float currentTime,
            float reloadDuration)
        {
            if (isReloading || magazineCapacity <= 0 || currentAmmo >= magazineCapacity)
            {
                return false;
            }

            isReloading = true;
            completionTime = currentTime + Math.Max(0.001f, reloadDuration);
            return true;
        }

        public bool IsComplete(float currentTime)
        {
            return isReloading && currentTime >= completionTime;
        }

        public bool TryComplete(float currentTime)
        {
            if (!IsComplete(currentTime))
            {
                return false;
            }

            isReloading = false;
            completionTime = 0f;
            return true;
        }

        public void Cancel()
        {
            isReloading = false;
            completionTime = 0f;
        }

        public void Reset()
        {
            Cancel();
        }
    }
}
