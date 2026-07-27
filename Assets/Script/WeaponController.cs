using Assets.Script;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Weapon))]
public class WeaponController : MonoBehaviour
{
    [Header("Weapon GameObjects")]
    public GameObject bulletSpawnPoint;
    public GameObject pickupsSpawnPoint;
    public GameObject bulletPrefab;
    public GameObject ammoPrefab;

    [Header("Fire Settings")]
    public float bulletSpeed = 500f;
    public float fireRate = 0.2f;
    public AudioClip shootSound; // The sound to play when the player fires
    public float shootVolume = 0.5f; // The volume at which to play the shoot sound

    private float lastShotTime;
    private bool isFiring;
    private bool isSingleFiring;

    [Header("Fire Modes")]
    public bool isAutoAvailible = false;
    public bool isBurstAvailible = false;
    public int burstSize = 3;
    public float burstInterval = 0.5f;
    public FireMode fireMode = FireMode.Auto;

    [Header("Reloading")]
    public int currentAmmo = 0;
    public float reloadTime = 1.0f;
    //Add Sound
    [SerializeField]
    private bool isReloading = false;

    [Header("Recoil")]
    public float recoilForce = 5f;
    public float maxRecoilAmount = 2f;
    public float recoilIncreaseAmount = 0.1f;
    public float recoilDecreaseAmount = 1f;
    public float recoilVerticalPlus = 1.5f;
    public float recoilVerticalMinus = 0.5f;

    private float currentRecoilAmount;

    [Header("Ammo Drop")]
    public float ammoImpulseSpeed = 10.0f; // The speed at which the ammo pickups are initially propelled
    public float ammoImpulseDuration = 1.0f; // The duration over which the initial impulse force is attenuated
    public float maxRotation = 45.0f; // The maximum rotation to apply to the ammo pickup
    public float maxOffset = 0.1f; // The maximum offset to apply to the ammo pickup
    public AudioClip ammoDropSound; // New field for the ammo drop sound
    public float ammoVolume = 0.3f; // The volume at which to play the shoot sound

    private bool isLeftRotated = false;

    [Header("NPC")]
    public bool isNPCControlled = false;
    public float npcTargetOffsetY = -0.5f;
    public Transform npcTarget;
    public Transform objectTarget;

    [Header("Controll")]
    public float rotationSpeed = 10f; // The speed of rotation, in degrees per second.

    private Vector3 mousePosition; // The position of the mouse in world space.

    [Header("Others")]
    //Others
    private AudioSource audioSource; // The AudioSource component to play the shoot sound
    private GlobalSystem globalSystem;
    private UIAmmoSystem ammoSystem;
    private Animator weapongAnimator;

    public Weapon weapon;

    public enum FireMode
    {
        Auto,
        Burst,
        Single
    }

    private void Awake()
    {
        weapon = GetComponent<Weapon>();
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        isNPCControlled = GetComponentInParent<NPCController>() != null;
        globalSystem = GameObject.FindGameObjectWithTag("System").GetComponent<GlobalSystem>();
        ammoSystem = globalSystem.UIAmmoSystem;
        currentAmmo = weapon.weaponAmmoMax;
        weapongAnimator = GetComponentInParent<Animator>();
        isReloading = false;
    }

    private void OnEnable()
    {
        isFiring = false;
        isSingleFiring = false;
    }

    private void Update()
    {
        if (!isNPCControlled)
        {
            if (Input.GetMouseButton(0))
            {
                FireWithModes();
            } else if (Input.GetButtonUp("Fire1"))
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
        RecoilDecrease();
    }

    public void FireWithModes()
    {
        switch (fireMode)
        {
            case FireMode.Auto:
                if (!isFiring)
                {
                    StartCoroutine(FireAuto());
                }
                break;
            case FireMode.Burst:
                if (!isFiring)
                {
                    StartCoroutine(FireBurst());
                }
                break;
            case FireMode.Single:
                if (!isFiring && !isSingleFiring)
                {
                    StartCoroutine(FireSingleShot());
                }
                break;
        }
    }

    IEnumerator FireSingleShot()
    {
        isFiring = true;
        isSingleFiring = true;
        Fire();
        yield return new WaitForSeconds(fireRate);
        isFiring = false;
        if (isNPCControlled)
        {
            isSingleFiring = false;
        }
    }

    IEnumerator FireBurst()
    {
        isFiring = true;
        int shotsFired = 0;

        while (shotsFired < burstSize)
        {
            Fire();
            shotsFired++;
            yield return new WaitForSeconds(fireRate);
        }

        yield return new WaitForSeconds(burstInterval);
        isFiring = false;
    }

    IEnumerator FireAuto()
    {
        isFiring = true;
        Fire();
        yield return new WaitForSeconds(fireRate);
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
                if (isAutoAvailible)
                {
                    fireMode = FireMode.Auto;
                }
                else
                {
                    fireMode = FireMode.Single;
                }
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

    private void FixedUpdate()
    {
        if (!isNPCControlled)
        {
            Vector3 direction = CalculateDirectionForPlayerMouse();

            float targetAngle = CalculateTargetAngle(direction);

            ChangeDirectWeaponByAngle(targetAngle);
            RotateWeaponWithRecoil(targetAngle);
        }
        else
        {
            if (npcTarget != null)
            {
                float targetAngle = GetTargetAngle(npcTarget);

                ChangeDirectWeaponByAngle(targetAngle);
                RotateWeaponWithRecoil(targetAngle);
            }
            else if(objectTarget != null)
            {
                float targetAngle = GetTargetAngle(objectTarget);

                ChangeDirectWeaponByAngle(targetAngle);
                rotateWeaponToDirectionWithoutZ();
            }
            else
            {
                rotateWeaponToDirectionWithoutZ();
            }
        }
    }

    public void rotateWeaponToDirectionWithoutZ()
    {
        if (transform.rotation.eulerAngles.z != (isLeftRotated ? 180 : 0))
        {
            RotateWeapon(isLeftRotated ? 180 : 0);
        }
    }

    public float GetTargetAngle(Transform target)
    {
        Vector3 direction = CalculateDirectionForObjects(target);
        direction = new Vector3(direction.x, direction.y + npcTargetOffsetY, direction.z);
        return CalculateTargetAngle(direction);
    }

    public void RecoilDecrease()
    {
        float timeSinceShot = Time.time - lastShotTime;
        float recoilDecrease = recoilDecreaseAmount * timeSinceShot;
        currentRecoilAmount = Mathf.Max(0f, currentRecoilAmount - recoilDecrease);
    }

    public void RotateWeapon(float targetAngle)
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, 0f, targetAngle), rotationSpeed * Time.deltaTime); ;
    }

    public void RotateWeaponWithRecoil(float targetAngle)
    {
        // Smoothly rotate towards the target angle over time.
        float recoil = Random.Range(-currentRecoilAmount * (!isLeftRotated ? recoilVerticalMinus : recoilVerticalPlus), currentRecoilAmount * (!isLeftRotated ? recoilVerticalPlus : recoilVerticalMinus));

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, 0f, targetAngle + recoil), rotationSpeed * Time.deltaTime); ;
    }

    public void ChangeDirectWeaponByAngle(float targetAngle)
    {
        if (((targetAngle > 90 || targetAngle < -90) && transform.localScale.y > 0) || targetAngle < 90 && targetAngle > -90 && transform.localScale.y < 0)
        {
            transform.localScale = new Vector3(transform.localScale.x, -transform.localScale.y, transform.localScale.z);
            isLeftRotated = transform.localScale.y < 0;
        }
    }

    public float CalculateTargetAngle(Vector3 target)
    {
        return Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;
    }

    public Vector3 CalculateDirectionForPlayerMouse()
    {
        var mouse_pos = Input.mousePosition;
        var object_pos = Camera.main.WorldToScreenPoint(transform.position);
        mouse_pos.x = mouse_pos.x - object_pos.x;
        mouse_pos.y = mouse_pos.y - object_pos.y;

        // Get the position of the mouse in world space.
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f; // Make sure the z-coordinate is zero.

        // Calculate the target rotation angle in degrees.
        Vector3 direction = mousePosition - transform.position;
        return direction;
    }

    public Vector3 CalculateDirectionForObjects(Transform objectTo)
    {
        Vector3 object1Position = transform.position;
        Vector3 object2Position = objectTo.position;

        // Calculate the direction vector from the root object to object2.
        Vector3 direction = object2Position - object1Position;

        return direction;
    }

    public void Fire()
    {
        if (currentAmmo > 0 && !isReloading)
        {
            audioSource.PlayOneShot(shootSound, shootVolume);

            //CountAmmo
            currentAmmo--;
            if (!isNPCControlled)
            {
                ammoSystem.PopAmmo(currentAmmo, weapon.weaponAmmoMax);
            }

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.nearClipPlane;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector3 direction = worldPos - transform.position;

            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.transform.position, transform.rotation);
            Rigidbody2D rb2d = bullet.GetComponent<Rigidbody2D>();

            rb2d.AddForce(transform.right * bulletSpeed);

            currentRecoilAmount += recoilIncreaseAmount;
            currentRecoilAmount = Mathf.Clamp(currentRecoilAmount, 0f, maxRecoilAmount);

            Vector3 ammoDropPosition = pickupsSpawnPoint.transform.position + new Vector3(0, 0, 3);
            Quaternion ammoDropRotation = Quaternion.Euler(0.0f, 0.0f, Random.Range(-maxRotation, maxRotation));
            ammoDropPosition += transform.up * Random.Range(-maxOffset, maxOffset);
            GameObject ammoDrop = Instantiate(ammoPrefab, ammoDropPosition, ammoDropRotation, globalSystem.garbadge);
            Vector2 forceDirection = -transform.up * ammoImpulseSpeed;
            ammoDrop.GetComponent<Rigidbody2D>().AddForce(forceDirection, ForceMode2D.Impulse);
            StartCoroutine(Tools.AttenuateAmmoImpulse(ammoDrop.GetComponent<Rigidbody2D>(), ammoImpulseDuration));

            AudioSource.PlayClipAtPoint(ammoDropSound, ammoDrop.transform.position, ammoVolume);

            lastShotTime = Time.time;
        }
    }

    public void SetNPCTarget(Transform target)
    {
        npcTarget = target != null ? target.transform : null;
    }
    public void SetObjectTarget(Transform target)
    {
        objectTarget = target != null ? target.transform : null;
    }

    public void Reload()
    {
        if (!isReloading)
        {
            isReloading = true;
            StartCoroutine(ReloadCoroutine());
        }
    }

    private IEnumerator ReloadCoroutine()
    {
        // Play the reloading animation here
        if (weapongAnimator != null)
        {
            weapongAnimator.enabled = true;
            weapongAnimator.SetFloat("ReloadSpeed", 1 / reloadTime);
            weapongAnimator.SetTrigger("Reload");
        }

        yield return new WaitForSeconds(reloadTime); // Wait for the duration of the reloading animation

        // Execute the actual reload logic here

        weapongAnimator.enabled = false;
        currentAmmo = weapon.weaponAmmoMax;
        if (!isNPCControlled) // Show UI only for Player
        {
            ammoSystem.ReloadAmmo(weapon.weaponAmmoMax);
        }
        
        isReloading = false; // Reset the reloading flag
    }

    public void WeaponChanged()
    {
        isReloading = false;
    }
}