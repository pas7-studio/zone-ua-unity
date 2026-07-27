using Assets.Script;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Weapon))]
[RequireComponent(typeof(AudioSource))]
public sealed class WeaponController : MonoBehaviour
{
    [Header("Weapon Objects")]
    [SerializeField] private GameObject bulletSpawnPoint;
    [SerializeField] private GameObject pickupsSpawnPoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject ammoPrefab;

    [Header("Fire Settings")]
    [SerializeField, Min(0f)] private float bulletSpeed = 500f;
    [SerializeField, Min(0.01f)] private float fireRate = 0.2f;
    [SerializeField] private AudioClip shootSound;
    [SerializeField, Range(0f, 1f)] private float shootVolume = 0.5f;

    [Header("Fire Modes")]
    [SerializeField] private bool isAutoAvailible;
    [SerializeField] private bool isBurstAvailible;
    [SerializeField, Min(1)] private int burstSize = 3;
    [SerializeField, Min(0f)] private float burstInterval = 0.5f;
    [SerializeField] private FireMode fireMode = FireMode.Auto;

    [Header("Reloading")]
    [SerializeField, Min(0)] private int currentAmmo;
    [SerializeField, Min(0.01f)] private float reloadTime = 1f;
    [SerializeField] private bool isReloading;

    [Header("Recoil")]
    [SerializeField, Min(0f)] private float recoilForce = 5f;
    [SerializeField, Min(0f)] private float maxRecoilAmount = 2f;
    [SerializeField, Min(0f)] private float recoilIncreaseAmount = 0.1f;
    [SerializeField, Min(0f)] private float recoilDecreaseAmount = 1f;
    [SerializeField, Min(0f)] private float recoilVerticalPlus = 1.5f;
    [SerializeField, Min(0f)] private float recoilVerticalMinus = 0.5f;

    [Header("Ammo Drop")]
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
    private bool isFiring;
    private bool isSingleFiring;
    private bool isLeftRotated;

    public enum FireMode
    {
        Auto,
        Burst,
        Single
    }

    public Weapon WeaponData => weapon;
    public int CurrentAmmo => currentAmmo;
    public bool SupportsAuto => isAutoAvailible;
    public bool SupportsBurst => isBurstAvailible;
    public bool IsReloading => isReloading;

    public FireMode CurrentFireMode
    {
        get => fireMode;
        set => fireMode = value;
    }

    public float RotationSpeed
    {
        get => rotationSpeed;
        set => rotationSpeed = Mathf.Max(0f, value);
    }

    private void Awake()
    {
        weapon = GetComponent<Weapon>();
        audioSource = GetComponent<AudioSource>();
        weaponAnimator = GetComponentInParent<Animator>();
        mainCamera = Camera.main;
        bulletSpawnTransform = bulletSpawnPoint != null ? bulletSpawnPoint.transform : null;
        pickupsSpawnTransform = pickupsSpawnPoint != null ? pickupsSpawnPoint.transform : null;
        isNPCControlled = GetComponentInParent<NPCController>() != null;
    }

    private void Start()
    {
        globalSystem = GlobalSystem.Instance;
        ammoSystem = globalSystem != null ? globalSystem.AmmoUI : null;
        currentAmmo = weapon.MaximumAmmo;
        isReloading = false;
    }

    private void OnEnable()
    {
        isFiring = false;
        isSingleFiring = false;
        nextShotTime = 0f;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isFiring = false;
        isReloading = false;
    }

    private void Update()
    {
        if (!isNPCControlled)
        {
            HandlePlayerInput();
        }

        UpdateAim();
        currentRecoilAmount = Mathf.MoveTowards(
            currentRecoilAmount,
            0f,
            recoilDecreaseAmount * Time.deltaTime);
    }

    private void HandlePlayerInput()
    {
        if (Input.GetMouseButton(0))
        {
            FireWithModes();
        }

        if (Input.GetButtonUp("Fire1"))
        {
            isSingleFiring = false;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            FireModeChange();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
    }

    private void UpdateAim()
    {
        if (isNPCControlled)
        {
            if (npcTarget != null)
            {
                float targetAngle = GetTargetAngle(npcTarget);
                ChangeDirectWeaponByAngle(targetAngle);
                RotateWeaponWithRecoil(targetAngle);
            }
            else if (objectTarget != null)
            {
                float targetAngle = GetTargetAngle(objectTarget);
                ChangeDirectWeaponByAngle(targetAngle);
                RotateWeapon(targetAngle);
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

        float playerTargetAngle = CalculateTargetAngle(direction);
        ChangeDirectWeaponByAngle(playerTargetAngle);
        RotateWeaponWithRecoil(playerTargetAngle);
    }

    public void FireWithModes()
    {
        if (isReloading || Time.time < nextShotTime)
        {
            return;
        }

        switch (fireMode)
        {
            case FireMode.Auto:
                TryFireSingleRound();
                break;

            case FireMode.Burst:
                if (!isFiring)
                {
                    StartCoroutine(FireBurstRoutine());
                }
                break;

            case FireMode.Single:
                if (!isSingleFiring)
                {
                    isSingleFiring = true;
                    TryFireSingleRound();

                    if (isNPCControlled)
                    {
                        isSingleFiring = false;
                    }
                }
                break;
        }
    }

    private bool TryFireSingleRound()
    {
        if (!TryFireRound())
        {
            return false;
        }

        nextShotTime = Time.time + fireRate;
        return true;
    }

    private IEnumerator FireBurstRoutine()
    {
        isFiring = true;

        for (int shot = 0; shot < burstSize; shot++)
        {
            if (isReloading || currentAmmo <= 0)
            {
                break;
            }

            TryFireRound();
            nextShotTime = Time.time + fireRate;
            yield return new WaitForSeconds(fireRate);
        }

        if (burstInterval > 0f)
        {
            yield return new WaitForSeconds(burstInterval);
        }

        isFiring = false;
    }

    public void FireModeChange()
    {
        switch (fireMode)
        {
            case FireMode.Auto:
                fireMode = FireMode.Single;
                break;

            case FireMode.Burst:
                fireMode = isAutoAvailible ? FireMode.Auto : FireMode.Single;
                break;

            case FireMode.Single:
                if (isBurstAvailible)
                {
                    fireMode = FireMode.Burst;
                }
                else if (isAutoAvailible)
                {
                    fireMode = FireMode.Auto;
                }
                break;
        }
    }

    public void Fire()
    {
        TryFireRound();
    }

    private bool TryFireRound()
    {
        if (currentAmmo <= 0 || isReloading || bulletPrefab == null || bulletSpawnTransform == null)
        {
            return false;
        }

        if (shootSound != null)
        {
            audioSource.PlayOneShot(shootSound, shootVolume);
        }

        currentAmmo--;
        if (!isNPCControlled)
        {
            ammoSystem?.PopAmmo(currentAmmo, weapon.MaximumAmmo);
        }

        GameObject bullet = Instantiate(
            bulletPrefab,
            bulletSpawnTransform.position,
            transform.rotation);

        if (bullet.TryGetComponent(out Rigidbody2D bulletBody))
        {
            bulletBody.AddForce(transform.right * bulletSpeed);
        }

        currentRecoilAmount = Mathf.Min(
            maxRecoilAmount,
            currentRecoilAmount + recoilIncreaseAmount + recoilForce * 0.001f);

        SpawnAmmoDrop();
        return true;
    }

    private void SpawnAmmoDrop()
    {
        if (ammoPrefab == null || pickupsSpawnTransform == null)
        {
            return;
        }

        globalSystem ??= GlobalSystem.Instance;

        Vector3 position = pickupsSpawnTransform.position;
        position.z += 3f;
        position += transform.up * Random.Range(-maxOffset, maxOffset);

        Quaternion rotation = Quaternion.Euler(
            0f,
            0f,
            Random.Range(-maxRotation, maxRotation));

        GameObject ammoDrop = Instantiate(
            ammoPrefab,
            position,
            rotation,
            globalSystem != null ? globalSystem.RuntimeContainer : null);

        if (ammoDrop.TryGetComponent(out Rigidbody2D body))
        {
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
        if (isReloading || currentAmmo >= weapon.MaximumAmmo)
        {
            return;
        }

        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;

        if (weaponAnimator != null)
        {
            weaponAnimator.enabled = true;
            weaponAnimator.SetFloat("ReloadSpeed", 1f / Mathf.Max(0.01f, reloadTime));
            weaponAnimator.SetTrigger("Reload");
        }

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = weapon.MaximumAmmo;
        if (!isNPCControlled)
        {
            ammoSystem?.ReloadAmmo(weapon.MaximumAmmo);
        }

        if (weaponAnimator != null)
        {
            weaponAnimator.enabled = false;
        }

        isReloading = false;
    }

    public void WeaponChanged()
    {
        StopAllCoroutines();
        isReloading = false;
        isFiring = false;
        isSingleFiring = false;
    }

    public void SetNPCTarget(Transform target)
    {
        npcTarget = target;
    }

    public void SetObjectTarget(Transform target)
    {
        objectTarget = target;
    }

    public float GetTargetAngle(Transform target)
    {
        Vector3 direction = CalculateDirectionForObjects(target);
        direction.y += npcTargetOffsetY;
        return CalculateTargetAngle(direction);
    }

    public float CalculateTargetAngle(Vector3 target)
    {
        return Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;
    }

    public Vector3 CalculateDirectionForPlayerMouse()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            return Vector3.zero;
        }

        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = transform.position.z;
        return mousePosition - transform.position;
    }

    public Vector3 CalculateDirectionForObjects(Transform target)
    {
        return target != null ? target.position - transform.position : Vector3.zero;
    }

    public void ChangeDirectWeaponByAngle(float targetAngle)
    {
        bool shouldFaceLeft = targetAngle > 90f || targetAngle < -90f;

        if (shouldFaceLeft == isLeftRotated)
        {
            return;
        }

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
        float min = -currentRecoilAmount * (isLeftRotated ? recoilVerticalPlus : recoilVerticalMinus);
        float max = currentRecoilAmount * (isLeftRotated ? recoilVerticalMinus : recoilVerticalPlus);
        RotateWeapon(targetAngle + Random.Range(min, max));
    }

    public void RecoilDecrease()
    {
        currentRecoilAmount = Mathf.MoveTowards(
            currentRecoilAmount,
            0f,
            recoilDecreaseAmount * Time.deltaTime);
    }

    public void rotateWeaponToDirectionWithoutZ()
    {
        RotateWeapon(isLeftRotated ? 180f : 0f);
    }

    private void OnValidate()
    {
        fireRate = Mathf.Max(0.01f, fireRate);
        reloadTime = Mathf.Max(0.01f, reloadTime);
        burstSize = Mathf.Max(1, burstSize);
        rotationSpeed = Mathf.Max(0f, rotationSpeed);
    }
}
