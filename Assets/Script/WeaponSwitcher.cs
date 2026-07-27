using System.Collections;
using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    public GameObject weapon1;
    public GameObject weapon2;

    [SerializeField]
    private float switchingTime = 1f;

    [SerializeField]
    private GameObject selectedWeapon;

    private Animator weapon1Animator;
    private Animator weapon2Animator;

    private bool isSwitching = false;

    private GlobalSystem globalSystem;
    private UIAmmoSystem ammoSystem;

    private void Start()
    {
        globalSystem = GameObject.FindGameObjectWithTag("System").GetComponent<GlobalSystem>();
        ammoSystem = globalSystem.UIAmmoSystem;
    }

    //rewrite this, code is shit
    private void Update()
    {
        // Only switch weapons if not already switching
        if (!isSwitching)
        {
            // Switch to weapon 1 when the player presses the 1 key
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                // Find the animator component on weapon 1
                weapon1Animator = weapon1.GetComponent<Animator>();
                weapon1Animator.enabled = true;

                selectedWeapon = weapon1;
                StartCoroutine(SwitchWeapon(weapon1, weapon1Animator));
            }

            // Switch to weapon 2 when the player presses the 2 key
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                // Find the animator component on weapon 2
                weapon2Animator = weapon2.GetComponent<Animator>();
                weapon2Animator.enabled = true;

                selectedWeapon = weapon2;
                StartCoroutine(SwitchWeapon(weapon2, weapon2Animator));
            }
        }

        // Turn off any active weapon when the player presses the Q key
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(SwitchWeapon(null, null));
        }
    }

    public void HideAllWeapons()
    {
        StartCoroutine(SwitchWeapon(null, null));
    }

    private IEnumerator SwitchWeapon(GameObject newWeapon, Animator newAnimator)
    {
        isSwitching = true;
        // Disable the current weapon
        if (weapon1.activeSelf)
        {
            weapon1.SetActive(false);
        }
        else if (weapon2.activeSelf)
        {
            weapon2.SetActive(false);
        }

        // Enable the new weapon, if any
        if (newWeapon != null)
        {
            newWeapon.SetActive(true);
        }

        // Play the weapon switch animation, if there is a new animator
        if (newAnimator != null)
        {
            newAnimator.SetTrigger("Switch");
        }

        yield return new WaitForSeconds(switchingTime);

        var newWeaponController = newWeapon.GetComponent<WeaponController>();
        newWeaponController.WeaponChanged();
        ammoSystem.SetMaximumAmmo(newWeaponController.weapon.weaponAmmoMax);
        ammoSystem.SetAmmo(newWeaponController.currentAmmo, newWeaponController.weapon.weaponAmmoMax);

        if (newAnimator != null)
        {
            newAnimator.enabled = false;
        }

        isSwitching = false;
    }
}