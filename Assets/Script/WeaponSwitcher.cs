using System;
using System.Collections;
using UnityEngine;

public sealed class WeaponSwitcher : MonoBehaviour
{
    private static readonly int SwitchHash = Animator.StringToHash("Switch");

    [Header("Weapons")]
    [SerializeField] private GameObject weapon1;
    [SerializeField] private GameObject weapon2;

    [Header("Switching")]
    [SerializeField, Min(0f)] private float switchingTime = 1f;
    [SerializeField, HideInInspector] private GameObject selectedWeapon;

    private readonly GameObject[] weapons = new GameObject[2];
    private readonly Animator[] animators = new Animator[2];
    private readonly WeaponController[] controllers = new WeaponController[2];

    private Coroutine switchRoutine;
    private bool isSwitching;

    public event Action<WeaponController> ActiveWeaponChanged;

    public GameObject SelectedWeapon => selectedWeapon;
    public WeaponController ActiveWeaponController { get; private set; }
    public bool IsSwitching => isSwitching;

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
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null && weapons[i].activeSelf)
            {
                SetActiveWeapon(weapons[i], controllers[i]);
                return;
            }
        }

        SetActiveWeapon(null, null);
    }

    public void RequestSwitch(int index)
    {
        if (isSwitching || index < 0 || index >= weapons.Length || weapons[index] == null)
        {
            return;
        }

        if (selectedWeapon == weapons[index])
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
        ActiveWeaponController?.StopFire();
        SetAllWeaponsActive(false);

        newWeapon.SetActive(true);
        SetActiveWeapon(newWeapon, newController);

        if (newAnimator != null)
        {
            newAnimator.enabled = true;
            newAnimator.SetTrigger(SwitchHash);
        }

        if (switchingTime > 0f)
        {
            yield return new WaitForSeconds(switchingTime);
        }

        newController?.WeaponChanged();

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

        ActiveWeaponController?.StopFire();
        isSwitching = false;
        SetAllWeaponsActive(false);
        SetActiveWeapon(null, null);
    }

    private void SetActiveWeapon(GameObject weaponObject, WeaponController controller)
    {
        if (selectedWeapon == weaponObject && ActiveWeaponController == controller)
        {
            return;
        }

        selectedWeapon = weaponObject;
        ActiveWeaponController = controller;
        ActiveWeaponChanged?.Invoke(controller);
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

    private void OnDisable()
    {
        ActiveWeaponController?.StopFire();
    }

    private void OnValidate()
    {
        switchingTime = Mathf.Max(0f, switchingTime);
    }
}
