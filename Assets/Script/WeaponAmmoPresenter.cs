using UnityEngine;

[DisallowMultipleComponent]
public sealed class WeaponAmmoPresenter : MonoBehaviour
{
    [Header("Sources")]
    [SerializeField, Tooltip("Weapon switcher that owns the currently active player weapon.")]
    private WeaponSwitcher weaponSwitcher;

    [SerializeField, Tooltip("Ammo HUD view updated from weapon events.")]
    private UIAmmoSystem ammoView;

    [Header("Visibility")]
    [SerializeField, Tooltip("Hide ammo HUD when no weapon is active.")]
    private bool hideWhenUnarmed = true;

    private WeaponController boundWeapon;

    private void Awake()
    {
        if (weaponSwitcher == null)
        {
            weaponSwitcher = GetComponentInParent<WeaponSwitcher>();
        }
    }

    private void OnEnable()
    {
        if (weaponSwitcher == null)
        {
            Debug.LogError($"{nameof(WeaponAmmoPresenter)} requires a {nameof(WeaponSwitcher)} reference.", this);
            enabled = false;
            return;
        }

        if (ammoView == null)
        {
            Debug.LogError($"{nameof(WeaponAmmoPresenter)} requires a {nameof(UIAmmoSystem)} reference.", this);
            enabled = false;
            return;
        }

        weaponSwitcher.ActiveWeaponChanged += HandleActiveWeaponChanged;
        Bind(weaponSwitcher.ActiveWeaponController);
    }

    private void OnDisable()
    {
        if (weaponSwitcher != null)
        {
            weaponSwitcher.ActiveWeaponChanged -= HandleActiveWeaponChanged;
        }

        UnbindCurrentWeapon();
    }

    private void HandleActiveWeaponChanged(WeaponController weaponController)
    {
        Bind(weaponController);
    }

    private void Bind(WeaponController weaponController)
    {
        if (boundWeapon == weaponController)
        {
            Refresh();
            return;
        }

        UnbindCurrentWeapon();
        boundWeapon = weaponController;

        if (boundWeapon == null)
        {
            if (hideWhenUnarmed)
            {
                ammoView.ShowHideUI(false);
            }
            else
            {
                ammoView.SetAmmo(0, 0);
            }

            return;
        }

        boundWeapon.AmmoChanged += HandleAmmoChanged;
        boundWeapon.ReloadCompleted += Refresh;
        ammoView.ShowHideUI(true);
        Refresh();
    }

    private void UnbindCurrentWeapon()
    {
        if (boundWeapon == null)
        {
            return;
        }

        boundWeapon.AmmoChanged -= HandleAmmoChanged;
        boundWeapon.ReloadCompleted -= Refresh;
        boundWeapon = null;
    }

    private void HandleAmmoChanged(int currentAmmo, int maximumAmmo)
    {
        ammoView.SetAmmo(currentAmmo, maximumAmmo);
    }

    private void Refresh()
    {
        if (boundWeapon == null || ammoView == null)
        {
            return;
        }

        ammoView.SetAmmo(boundWeapon.CurrentAmmo, boundWeapon.MagazineCapacity);
    }
}
