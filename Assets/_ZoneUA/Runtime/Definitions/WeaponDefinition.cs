using UnityEngine;

namespace ZoneUA.Combat
{
    public enum WeaponFireMode
    {
        Single,
        Burst,
        Automatic
    }

    [CreateAssetMenu(fileName = "Weapon", menuName = "Zone UA/Combat/Weapon Definition")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string displayName = "Weapon";

        [Header("Projectile")]
        [SerializeField] private ProjectileDefinition projectile;

        [Header("Magazine")]
        [SerializeField, Min(1)] private int magazineCapacity = 30;
        [SerializeField, Min(0.01f)] private float reloadDuration = 1f;

        [Header("Fire")]
        [SerializeField, Min(0.01f), Tooltip("Seconds between consecutive shots.")]
        private float shotInterval = 0.2f;

        [SerializeField] private bool supportsSingle = true;
        [SerializeField] private bool supportsBurst;
        [SerializeField] private bool supportsAutomatic;
        [SerializeField, Min(2)] private int burstSize = 3;
        [SerializeField, Min(0f)] private float burstCooldown = 0.5f;
        [SerializeField] private WeaponFireMode defaultFireMode = WeaponFireMode.Single;

        [Header("Accuracy")]
        [SerializeField, Min(0f), Tooltip("Base projectile spread in degrees.")]
        private float baseSpread;
        [SerializeField, Min(0f)] private float maximumSpread = 5f;
        [SerializeField, Min(0f)] private float spreadPerShot = 0.25f;
        [SerializeField, Min(0f)] private float spreadRecoveryPerSecond = 2f;

        [Header("Recoil")]
        [SerializeField, Min(0f)] private float weaponRecoil = 1f;
        [SerializeField, Min(0f)] private float cameraRecoil;

        [Header("Audio")]
        [SerializeField] private AudioClip shotClip;
        [SerializeField] private AudioClip reloadClip;
        [SerializeField] private AudioClip emptyClip;
        [SerializeField, Range(0f, 1f)] private float audioVolume = 0.7f;

        public string DisplayName => displayName;
        public ProjectileDefinition Projectile => projectile;
        public int MagazineCapacity => magazineCapacity;
        public float ReloadDuration => reloadDuration;
        public float ShotInterval => shotInterval;
        public bool SupportsSingle => supportsSingle;
        public bool SupportsBurst => supportsBurst;
        public bool SupportsAutomatic => supportsAutomatic;
        public int BurstSize => burstSize;
        public float BurstCooldown => burstCooldown;
        public WeaponFireMode DefaultFireMode => defaultFireMode;
        public float BaseSpread => baseSpread;
        public float MaximumSpread => maximumSpread;
        public float SpreadPerShot => spreadPerShot;
        public float SpreadRecoveryPerSecond => spreadRecoveryPerSecond;
        public float WeaponRecoil => weaponRecoil;
        public float CameraRecoil => cameraRecoil;
        public AudioClip ShotClip => shotClip;
        public AudioClip ReloadClip => reloadClip;
        public AudioClip EmptyClip => emptyClip;
        public float AudioVolume => audioVolume;

        public bool Supports(WeaponFireMode mode)
        {
            return mode switch
            {
                WeaponFireMode.Single => supportsSingle,
                WeaponFireMode.Burst => supportsBurst,
                WeaponFireMode.Automatic => supportsAutomatic,
                _ => false
            };
        }

        private void OnValidate()
        {
            displayName = string.IsNullOrWhiteSpace(displayName) ? name : displayName.Trim();
            magazineCapacity = Mathf.Max(1, magazineCapacity);
            reloadDuration = Mathf.Max(0.01f, reloadDuration);
            shotInterval = Mathf.Max(0.01f, shotInterval);
            burstSize = Mathf.Max(2, burstSize);
            baseSpread = Mathf.Max(0f, baseSpread);
            maximumSpread = Mathf.Max(baseSpread, maximumSpread);
            spreadPerShot = Mathf.Max(0f, spreadPerShot);
            spreadRecoveryPerSecond = Mathf.Max(0f, spreadRecoveryPerSecond);

            if (!supportsSingle && !supportsBurst && !supportsAutomatic)
            {
                supportsSingle = true;
            }

            if (!Supports(defaultFireMode))
            {
                defaultFireMode = supportsSingle
                    ? WeaponFireMode.Single
                    : supportsBurst
                        ? WeaponFireMode.Burst
                        : WeaponFireMode.Automatic;
            }
        }
    }
}
