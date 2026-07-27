using System;
using UnityEngine;

namespace ZoneUA.Combat
{
    public interface IWeaponCommands
    {
        event Action<int, int> AmmoChanged;
        event Action ReloadStarted;
        event Action ReloadCompleted;
        event Action<WeaponFireMode> FireModeChanged;

        int CurrentAmmo { get; }
        int MagazineCapacity { get; }
        bool IsReloading { get; }
        WeaponFireMode CurrentMode { get; }

        void StartFire();
        void StopFire();
        void Reload();
        void SetAimTarget(Transform target);
        void SwitchFireMode();
    }
}
