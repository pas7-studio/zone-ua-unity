using UnityEngine;

namespace ZoneUA.Combat
{
    [DisallowMultipleComponent]
    public sealed class WeaponInput : MonoBehaviour
    {
        [SerializeField, Tooltip("Component implementing IWeaponCommands.")]
        private MonoBehaviour commandSource;

        [Header("Legacy Input Bridge")]
        [SerializeField] private string fireButton = "Fire1";
        [SerializeField] private KeyCode reloadKey = KeyCode.R;
        [SerializeField] private KeyCode switchModeKey = KeyCode.B;

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

        private void Update()
        {
            if (commands == null)
            {
                return;
            }

            if (Input.GetButtonDown(fireButton)) commands.StartFire();
            if (Input.GetButtonUp(fireButton)) commands.StopFire();
            if (Input.GetKeyDown(reloadKey)) commands.Reload();
            if (Input.GetKeyDown(switchModeKey)) commands.SwitchFireMode();
        }

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
