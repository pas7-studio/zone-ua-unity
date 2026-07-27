using System.Collections;
using UnityEngine;

public sealed class WeaponSwitcher : MonoBehaviour
{
    [Header("Weapons")]
    [SerializeField] private GameObject weapon1;
    [SerializeField] private GameObject weapon2;

    [Header("Switching")]
    [SerializeField, Min(0f)] private float switchingTime = 1f;
    [SerializeField] private GameObject selectedWeapon;

    private readonly GameObject[] weapons = new GameObject[2];
    private readonly Animator[] animators = new Animator[2];
    private readonly WeaponController[] controllers = new WeaponController[2];

    private UIAmmoSystem ammoSystem;
    private Coroutine switchRoutine;
    private bool isSwitching;

    private void Awake()
    {
        weapons[0] = weapon1;
        weapons[1] = weapon2;

        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] == null)
            {
                continue;
            }

            animators[i] = weapons[i].GetComponent<Animator>();
            controllers[i] = weapons[i].GetComponent<WeaponController>();
        }
    }

    private void Start()
    {
        ammoSystem = GlobalSystem.Instance != null ? GlobalSystem.Instance.AmmoUI : null;

        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null && weapons[i].activeSelf)
            {
                selectedWeapon = weapons[i];
                RefreshAmmoUI(controllers[i]);
                break;
            }
        }
    }

    private void Update()
    {
        if (!isSwitching)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                RequestSwitch(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                RequestSwitch(1);
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            HideAllWeapons();
        }
    }

    private void RequestSwitch(int index)
    {
        if (index < 0 || index >= weapons.Length || weapons[index] == null)
        {
            return;
        }

        if (switchRoutine != null)
        {
            StopCoroutine(switchRoutine);
        }

        switchRoutine = StartCoroutine(SwitchWeaponRoutine(
            weapons[index],
            animators[index],
            controllers[index]));
    }

    private IEnumerator SwitchWeaponRoutine(
        GameObject newWeapon,
        Animator newAnimator,
        WeaponController newController)
    {
        isSwitching = true;
        SetAllWeaponsActive(false);

        newWeapon.SetActive(true);
        selectedWeapon = newWeapon;

        if (newAnimator != null)
        {
            newAnimator.enabled = true;
            newAnimator.SetTrigger("Switch");
        }

        if (switchingTime > 0f)
        {
            yield return new WaitForSeconds(switchingTime);
        }

        newController?.WeaponChanged();
        RefreshAmmoUI(newController);

        if (newAnimator != null)
        {
            newAnimator.enabled = false;
        }

        isSwitching = false;
        switchRoutine = null;
    }

    public void HideAllWeapons()
    {
        if (switchRoutine != null)
        {
            StopCoroutine(switchRoutine);
            switchRoutine = null;
        }

        isSwitching = false;
        selectedWeapon = null;
        SetAllWeaponsActive(false);
    }

    private void SetAllWeaponsActive(bool state)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
            {
                weapons[i].SetActive(state);
            }
        }
    }

    private void RefreshAmmoUI(WeaponController controller)
    {
        if (ammoSystem == null || controller == null || controller.WeaponData == null)
        {
            return;
        }

        int maximumAmmo = controller.WeaponData.MaximumAmmo;
        ammoSystem.SetMaximumAmmo(maximumAmmo);
        ammoSystem.SetAmmo(controller.CurrentAmmo, maximumAmmo);
    }
}
