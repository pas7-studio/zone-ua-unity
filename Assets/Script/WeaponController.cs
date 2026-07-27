using Assets.Script;
using System;
using UnityEngine;
using ZoneUA.Combat;

[RequireComponent(typeof(Weapon))]
[RequireComponent(typeof(AudioSource))]
public sealed class WeaponController : MonoBehaviour, IWeaponCommands, IWeaponInputOwnership
{
    private static readonly int ReloadSpeedHash = Animator.StringToHash("ReloadSpeed");
    private static readonly int ReloadHash = Animator.StringToHash("Reload");

    [Header("Modular Components")]
    [SerializeField, Tooltip("Optional projectile spawning module. Legacy spawning remains available during migration.")]
    private ProjectileSpawner projectileSpawner;
    [SerializeField, Tooltip("Optional shell ejection module. Legacy shell spawning remains available during migration.")]
    private ShellEjector shellEjector;
    [SerializeField, Tooltip("Optional weapon audio module. The local AudioSource remains a fallback.")]
    private WeaponAudio weaponAudio;
    [SerializeField, Tooltip("Optional recoil/spread module. Legacy recoil fields remain a fallback.")]
    private WeaponRecoil weaponRecoil;

    [Header("Weapon Objects (Legacy Fallback)")]
    [SerializeField] private GameObject bulletSpawnPoint;
    [SerializeField] private GameObject pickupsSpawnPoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject ammoPrefab;

    [Header("Legacy Fire Settings Fallback")]
    [SerializeField, Min(0f)] private float bulletSpeed = 500f;
    [SerializeField, Min(0.01f)] private float fireRate = 0.2f;
    [SerializeField] private AudioClip shootSound;
    [SerializeField, Range(0f, 1f)] private float shootVolume = 0.5f;

    [Header("Legacy Fire Modes Fallback")]
    [SerializeField] private bool isAutoAvailible;
    [SerializeField] private bool isBurstAvailible;
    [SerializeField, Min(1)] private int burstSize = 3;
    [SerializeField, Min(0f)] private float burstInterval = 0.5f;
    [SerializeField] private FireMode fireMode = FireMode.Auto;

    [Header("Runtime State")]
    [SerializeField, HideInInspector] private int currentAmmo;
    [SerializeField, HideInInspector] private bool isReloading;

    [Header("Legacy Reload Fallback")]
    [SerializeField, Min(0.01f)] private float reloadTime = 1f;

    [Header("Legacy Recoil Fallback")]
    [SerializeField, Min(0f)] private float recoilForce = 5f;
    [SerializeField, Min(0f)] private float maxRecoilAmount = 2f;
    [SerializeField, Min(0f)] private float recoilIncreaseAmount = 0.1f;
    [SerializeField, Min(0f)] private float recoilDecreaseAmount = 1f;
    [SerializeField, Min(0f)] private float recoilVerticalPlus = 1.5f;
    [SerializeField, Min(0f)] private float recoilVerticalMinus = 0.5f;

    [Header("Legacy Ammo Drop Fallback")]
    [SerializeField, Min(0f)] private float ammoImpulseSpeed = 10f;
    [SerializeField, Min(0f)] private float ammoImpulseDuration = 1f;
    [SerializeField, Range(0f, 180f)] private float maxRotation = 45f;
    [SerializeField, Min(0f)] private float maxOffset = 0.1f;
    [SerializeField] private AudioClip ammoDropSound;
    [SerializeField, Range(0f, 1f)] private float ammoVolume = 0.3f;

    [Header("NPC")]
    [SerializeField] private bool isNPCControlled;
    [SerializeField] private float npcTargetOffsetY = -0.5f;
    [SerializeField] private Transform npcTarget;
    [SerializeField] private Transform objectTarget;

    [Header("Control")]
    [SerializeField, Min(0f)] private float rotationSpeed = 10f;

    private AudioSource audioSource;
    private GlobalSystem globalSystem;
    private UIAmmoSystem ammoSystem;
    private Animator weaponAnimator;
    private Camera mainCamera;
    private Weapon weapon;
    private Transform bulletSpawnTransform;
    private Transform pickupsSpawnTransform;

    private float currentRecoilAmount;
    private float nextShotTime;
    private float reloadCompleteTime;
    private float burstCooldownUntil;
    private int burstShotsRemaining;
    private bool triggerHeld;
    private bool singleConsumed;
    private bool isLeftRotated;
    private bool externalInputEnabled;

    public enum FireMode
    {
        Auto,
        Burst,
        Single
    }

    public event Action<int, int> AmmoChanged;
    public event Action ReloadStarted;
    public event Action ReloadCompleted;
    public event Action<WeaponFireMode> FireModeChanged;

    public Weapon WeaponData => weapon;
    public int CurrentAmmo => currentAmmo;
    public int MagazineCapacity => weapon != null ? weapon.MaximumAmmo : 0;
    public bool SupportsAuto => SupportsMode(FireMode.Auto);
    public bool SupportsBurst => SupportsMode(FireMode.Burst);
    public bool IsReloading => isReloading;
    public WeaponFireMode CurrentMode => ToDefinitionMode(fireMode);

    public FireMode CurrentFireMode
    {
        get => fireMode;
        set => SetFireMode(value);
    }

    public float RotationSpeed
    {
        get => rotationSpeed;
        set => rotationSpeed = Mathf.Max(0f, value);
    }

    private WeaponDefinition Definition => weapon != null ? weapon.Definition : null;

    private void Awake()
    {
        weapon = GetComponent<Weapon>();
        audioSource = GetComponent<AudioSource>();
        weaponAnimator = GetComponentInParent<Animator>();
        mainCamera = Camera.main;
        bulletSpawnTransform = bulletSpawnPoint != null ? bulletSpawnPoint.transform : null;
        pickupsSpawnTransform = pickupsSpawnPoint != null ? pickupsSpawnPoint.transform : null;
        isNPCControlled = GetComponentInParent<NPCController>() != null;

        projectileSpawner ??= GetComponent<ProjectileSpawner>();
        shellEjector ??= GetComponent<ShellEjector>();
        weaponAudio ??= GetComponent<WeaponAudio>();
        weaponRecoil ??= GetComponent<WeaponRecoil>();

        if (projectileSpawner != null && projectileSpawner.Muzzle == null && bulletSpawnTransform != null)
        {
            projectileSpawner.SetMuzzle(bulletSpawnTransform);
        }

        ApplyDefinitionDefaults();
    }

    private void Start()
    {
        globalSystem = GlobalSystem.Instance;
        ammoSystem = globalSystem != null ? globalSystem.AmmoUI : null;
        currentAmmo = MagazineCapacity;
        isReloading = false;
        RaiseAmmoChanged();
    }

    private void OnEnable()
    {
        ResetTransientState();
    }

    private void OnDisable()
    {
        ResetTransientState();
        isReloading = false;
    }

    private void Update()
    {
        if (!isNPCControlled && !externalInputEnabled)
        {
            HandleLegacyPlayerInput();
        }

        TickReload();
        TickFire();
        UpdateAim();

        if (weaponRecoil != null)
        {
            weaponRecoil.Tick(Definition, Time.deltaTime);
        }
        else
        {
            currentRecoilAmount = Mathf.MoveTowards(
                currentRecoilAmount,
                0f,
                recoilDecreaseAmount * Time.deltaTime);
        }
    }

    private void HandleLegacyPlayerInput()
    {
        if (Input.GetButtonDown("Fire1")) StartFire();
        if (Input.GetButtonUp("Fire1")) StopFire();
        if (Input.GetKeyDown(KeyCode.B)) SwitchFireMode();
        if (Input.GetKeyDown(KeyCode.R)) Reload();
    }

    public void SetExternalInputEnabled(bool enabled)
    {
        externalInputEnabled = enabled;
        if (!enabled)
        {
            StopFire();
        }
    }

    public void StartFire()
    {
        if (triggerHeld)
        {
            return;
        }

        triggerHeld = true;
        singleConsumed = false;

        if (fireMode == FireMode.Burst)
        {
            TryBeginBurst();
        }
    }

    public void StopFire()
    {
        triggerHeld = false;
        singleConsumed = false;
    }

    public void SetAimTarget(Transform target)
    {
        npcTarget = target;
    }

    public void SwitchFireMode()
    {
        SetFireMode(GetNextSupportedMode(fireMode));
    }

    public void FireModeChange() => SwitchFireMode();

    public void FireWithModes()
    {
        if (isReloading)
        {
            return;
        }

        if (fireMode == FireMode.Burst)
        {
            TryBeginBurst();
        }
        else
        {
            TryFireSingleRound();
        }
    }

    public void Fire() => TryFireRound();

    private void TickFire()
    {
        if (isReloading)
        {
            return;
        }

        if (burstShotsRemaining > 0)
        {
            if (Time.time >= nextShotTime)
            {
                if (TryFireRound())
                {
                    burstShotsRemaining--;
                    nextShotTime = Time.time + ShotInterval;
                }
                else
                {
                    burstShotsRemaining = 0;
                }

                if (burstShotsRemaining == 0)
                {
                    burstCooldownUntil = Time.time + BurstCooldown;
                }
            }

            return;
        }

        if (!triggerHeld)
        {
            return;
        }

        switch (fireMode)
        {
            case FireMode.Auto:
                if (Time.time >= nextShotTime)
                {
                    TryFireSingleRound();
                }
                break;

            case FireMode.Single:
                if (!singleConsumed)
                {
                    singleConsumed = true;
                    TryFireSingleRound();
                }
                break;

            case FireMode.Burst:
                TryBeginBurst();
                break;
        }
    }

    private bool TryBeginBurst()
    {
        if (!SupportsMode(FireMode.Burst) || burstShotsRemaining > 0 || Time.time < burstCooldownUntil)
        {
            return false;
        }

        burstShotsRemaining = BurstSize;
        nextShotTime = Mathf.Max(nextShotTime, Time.time);
        return true;
    }

    private bool TryFireSingleRound()
    {
        if (Time.time < nextShotTime || !TryFireRound())
        {
            return false;
        }

        nextShotTime = Time.time + ShotInterval;
        return true;
    }

    private bool TryFireRound()
    {
        if (currentAmmo <= 0)
        {
            PlayEmptyAudio();
            return false;
        }

        if (isReloading)
        {
            return false;
        }

        ProjectileDefinition projectile = weapon != null ? weapon.Projectile : null;
        GameObject bullet = SpawnProjectile(projectile);
        if (bullet == null)
        {
            return false;
        }

        if (bullet.TryGetComponent(out Bullet bulletComponent))
        {
            bulletComponent.Configure(projectile, gameObject, transform.root.gameObject);
        }

        currentAmmo--;
        RaiseAmmoChanged();
        PlayShotAudio();
        RegisterRecoil();
        EjectShell();
        return true;
    }

    private GameObject SpawnProjectile(ProjectileDefinition projectile)
    {
        if (projectileSpawner != null)
        {
            return projectileSpawner.Spawn(projectile, bulletPrefab, bulletSpeed);
        }

        GameObject projectilePrefab = projectile != null && projectile.Prefab != null
            ? projectile.Prefab
            : bulletPrefab;

        if (projectilePrefab == null || bulletSpawnTransform == null)
        {
            return null;
        }

        globalSystem ??= GlobalSystem.Instance;
        GameObject bullet = globalSystem != null
            ? globalSystem.Spawn(projectilePrefab, bulletSpawnTransform.position, transform.rotation)
            : Instantiate(projectilePrefab, bulletSpawnTransform.position, transform.rotation);

        if (bullet != null && bullet.TryGetComponent(out Rigidbody2D bulletBody))
        {
            bulletBody.linearVelocity = transform.right * ProjectileSpeed;
            if (projectile != null && projectile.ContinuousCollision)
            {
                bulletBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }
        }

        return bullet;
    }

    private void RegisterRecoil()
    {
        if (weaponRecoil != null)
        {
            weaponRecoil.RegisterShot(Definition);
            return;
        }

        currentRecoilAmount = Mathf.Min(
            maxRecoilAmount,
            currentRecoilAmount + recoilIncreaseAmount + recoilForce * 0.001f);
    }

    private void EjectShell()
    {
        if (shellEjector != null)
        {
            shellEjector.Eject();
            return;
        }

        SpawnLegacyAmmoDrop();
    }

    private void PlayShotAudio()
    {
        if (weaponAudio != null)
        {
            weaponAudio.PlayShot(Definition);
            return;
        }

        AudioClip clip = Definition != null && Definition.ShotClip != null
            ? Definition.ShotClip
            : shootSound;
        float volume = Definition != null ? Definition.AudioVolume : shootVolume;

        if (clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    private void PlayReloadAudio()
    {
        weaponAudio?.PlayReload(Definition);
    }

    private void PlayEmptyAudio()
    {
        weaponAudio?.PlayEmpty(Definition);
    }

    private void SpawnLegacyAmmoDrop()
    {
        if (ammoPrefab == null || pickupsSpawnTransform == null)
        {
            return;
        }

        globalSystem ??= GlobalSystem.Instance;
        Vector3 position = pickupsSpawnTransform.position +
                           transform.up * UnityEngine.Random.Range(-maxOffset, maxOffset);
        Quaternion rotation = Quaternion.Euler(
            0f,
            0f,
            UnityEngine.Random.Range(-maxRotation, maxRotation));

        GameObject ammoDrop = globalSystem != null
            ? globalSystem.Spawn(ammoPrefab, position, rotation, globalSystem.RuntimeContainer)
            : Instantiate(ammoPrefab, position, rotation);

        if (ammoDrop != null && ammoDrop.TryGetComponent(out Rigidbody2D body))
        {
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.AddForce(-transform.up * ammoImpulseSpeed, ForceMode2D.Impulse);
            StartCoroutine(Tools.AttenuateVelocity(body, ammoImpulseDuration));
        }

        if (ammoDropSound != null)
        {
            audioSource.PlayOneShot(ammoDropSound, ammoVolume);
        }
    }

    public void Reload()
    {
        if (isReloading || currentAmmo >= MagazineCapacity)
        {
            return;
        }

        StopFire();
        burstShotsRemaining = 0;
        isReloading = true;
        reloadCompleteTime = Time.time + ReloadDuration;

        if (weaponAnimator != null)
        {
            weaponAnimator.enabled = true;
            weaponAnimator.SetFloat(ReloadSpeedHash, 1f / ReloadDuration);
            weaponAnimator.SetTrigger(ReloadHash);
        }

        PlayReloadAudio();
        ReloadStarted?.Invoke();
    }

    private void TickReload()
    {
        if (!isReloading || Time.time < reloadCompleteTime)
        {
            return;
        }

        currentAmmo = MagazineCapacity;
        isReloading = false;

        if (weaponAnimator != null)
        {
            weaponAnimator.enabled = false;
        }

        RaiseAmmoChanged();
        ReloadCompleted?.Invoke();
    }

    public void WeaponChanged()
    {
        ResetTransientState();
        isReloading = false;
    }

    public void SetNPCTarget(Transform target) => SetAimTarget(target);
    public void SetObjectTarget(Transform target) => objectTarget = target;

    private void UpdateAim()
    {
        if (isNPCControlled)
        {
            Transform target = npcTarget != null ? npcTarget : objectTarget;
            if (target != null)
            {
                float angle = GetTargetAngle(target);
                ChangeDirectWeaponByAngle(angle);
                if (npcTarget != null) RotateWeaponWithRecoil(angle); else RotateWeapon(angle);
            }
            else
            {
                RotateWeapon(isLeftRotated ? 180f : 0f);
            }
            return;
        }

        Vector3 direction = CalculateDirectionForPlayerMouse();
        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        float targetAngle = CalculateTargetAngle(direction);
        ChangeDirectWeaponByAngle(targetAngle);
        RotateWeaponWithRecoil(targetAngle);
    }

    public float GetTargetAngle(Transform target)
    {
        Vector3 direction = CalculateDirectionForObjects(target);
        direction.y += npcTargetOffsetY;
        return CalculateTargetAngle(direction);
    }

    public float CalculateTargetAngle(Vector3 target) =>
        Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;

    public Vector3 CalculateDirectionForPlayerMouse()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return Vector3.zero;

        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = transform.position.z;
        return mousePosition - transform.position;
    }

    public Vector3 CalculateDirectionForObjects(Transform target) =>
        target != null ? target.position - transform.position : Vector3.zero;

    public void ChangeDirectWeaponByAngle(float targetAngle)
    {
        bool shouldFaceLeft = targetAngle > 90f || targetAngle < -90f;
        if (shouldFaceLeft == isLeftRotated) return;

        Vector3 scale = transform.localScale;
        scale.y = -scale.y;
        transform.localScale = scale;
        isLeftRotated = shouldFaceLeft;
    }

    public void RotateWeapon(float targetAngle)
    {
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
        float interpolation = 1f - Mathf.Exp(-rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, interpolation);
    }

    public void RotateWeaponWithRecoil(float targetAngle)
    {
        if (weaponRecoil != null)
        {
            RotateWeapon(weaponRecoil.ApplyToAngle(targetAngle, isLeftRotated));
            return;
        }

        float minimum = -currentRecoilAmount *
                        (isLeftRotated ? recoilVerticalPlus : recoilVerticalMinus);
        float maximum = currentRecoilAmount *
                        (isLeftRotated ? recoilVerticalMinus : recoilVerticalPlus);
        RotateWeapon(targetAngle + UnityEngine.Random.Range(minimum, maximum));
    }

    public void RecoilDecrease()
    {
        if (weaponRecoil != null)
        {
            weaponRecoil.Tick(Definition, Time.deltaTime);
            return;
        }

        currentRecoilAmount = Mathf.MoveTowards(
            currentRecoilAmount,
            0f,
            recoilDecreaseAmount * Time.deltaTime);
    }

    public void rotateWeaponToDirectionWithoutZ() =>
        RotateWeapon(isLeftRotated ? 180f : 0f);

    private void SetFireMode(FireMode requested)
    {
        FireMode valid = SupportsMode(requested)
            ? requested
            : GetNextSupportedMode(requested);
        if (fireMode == valid) return;

        fireMode = valid;
        StopFire();
        burstShotsRemaining = 0;
        FireModeChanged?.Invoke(ToDefinitionMode(fireMode));
    }

    private FireMode GetNextSupportedMode(FireMode current)
    {
        FireMode[] order = { FireMode.Single, FireMode.Burst, FireMode.Auto };
        int start = Array.IndexOf(order, current);

        for (int offset = 1; offset <= order.Length; offset++)
        {
            FireMode candidate = order[(start + offset + order.Length) % order.Length];
            if (SupportsMode(candidate)) return candidate;
        }

        return FireMode.Single;
    }

    private bool SupportsMode(FireMode mode)
    {
        if (Definition != null)
        {
            return Definition.Supports(ToDefinitionMode(mode));
        }

        return mode switch
        {
            FireMode.Single => true,
            FireMode.Burst => isBurstAvailible,
            FireMode.Auto => isAutoAvailible,
            _ => false
        };
    }

    private void ApplyDefinitionDefaults()
    {
        if (Definition == null)
        {
            if (!SupportsMode(fireMode)) fireMode = FireMode.Single;
            return;
        }

        fireMode = FromDefinitionMode(Definition.DefaultFireMode);
        recoilIncreaseAmount = Definition.SpreadPerShot;
        maxRecoilAmount = Definition.MaximumSpread;
        recoilDecreaseAmount = Definition.SpreadRecoveryPerSecond;
    }

    private void ResetTransientState()
    {
        triggerHeld = false;
        singleConsumed = false;
        burstShotsRemaining = 0;
        nextShotTime = 0f;
        burstCooldownUntil = 0f;
        weaponRecoil?.ResetState();
        currentRecoilAmount = 0f;
    }

    private void RaiseAmmoChanged()
    {
        AmmoChanged?.Invoke(currentAmmo, MagazineCapacity);
        if (!isNPCControlled && ammoSystem != null)
        {
            ammoSystem.PopAmmo(currentAmmo, MagazineCapacity);
        }
    }

    private float ShotInterval => Definition != null ? Definition.ShotInterval : fireRate;
    private float ReloadDuration => Mathf.Max(
        0.01f,
        Definition != null ? Definition.ReloadDuration : reloadTime);
    private int BurstSize => Definition != null ? Definition.BurstSize : burstSize;
    private float BurstCooldown => Definition != null ? Definition.BurstCooldown : burstInterval;
    private float ProjectileSpeed => weapon != null && weapon.Projectile != null
        ? weapon.Projectile.Speed
        : bulletSpeed;

    private static WeaponFireMode ToDefinitionMode(FireMode mode)
    {
        return mode switch
        {
            FireMode.Auto => WeaponFireMode.Automatic,
            FireMode.Burst => WeaponFireMode.Burst,
            _ => WeaponFireMode.Single
        };
    }

    private static FireMode FromDefinitionMode(WeaponFireMode mode)
    {
        return mode switch
        {
            WeaponFireMode.Automatic => FireMode.Auto,
            WeaponFireMode.Burst => FireMode.Burst,
            _ => FireMode.Single
        };
    }

    private void OnValidate()
    {
        fireRate = Mathf.Max(0.01f, fireRate);
        reloadTime = Mathf.Max(0.01f, reloadTime);
        burstSize = Mathf.Max(1, burstSize);
        rotationSpeed = Mathf.Max(0f, rotationSpeed);
    }
}
