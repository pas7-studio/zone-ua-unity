using UnityEngine;

namespace ZoneUA.Combat
{
    [DisallowMultipleComponent]
    public sealed class WeaponInput : MonoBehaviour
    {
        [SerializeField, Tooltip("Component implementing IWeaponCommands.")]
        private MonoBehaviour commandSource;

        private IWeaponCommands commands;
        private IWeaponInputOwnership inputOwnership;

        private void Awake() => ResolveSource();

        private void OnEnable()
        {
            ResolveSource();
            inputOwnership?.SetExternalInputEnabled(true);
        }

        private void OnDisable()
        {
            commands?.StopFire();
            inputOwnership?.SetExternalInputEnabled(false);
        }

        public void StartFire() => commands?.StartFire();
        public void StopFire() => commands?.StopFire();
        public void Reload() => commands?.Reload();
        public void SwitchFireMode() => commands?.SwitchFireMode();

        private void ResolveSource()
        {
            if (commandSource == null)
            {
                MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
                for (int i = 0; i < behaviours.Length; i++)
                {
                    if (behaviours[i] is IWeaponCommands)
                    {
                        commandSource = behaviours[i];
                        break;
                    }
                }
            }

            commands = commandSource as IWeaponCommands;
            inputOwnership = commandSource as IWeaponInputOwnership;

            if (commands == null)
            {
                Debug.LogError($"{nameof(WeaponInput)} requires a component implementing {nameof(IWeaponCommands)}.", this);
                enabled = false;
            }
        }
    }
}
