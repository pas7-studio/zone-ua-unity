using UnityEngine;
using UnityEngine.InputSystem;
using ZoneUA.Combat;
using ZoneUA.Input;

[DisallowMultipleComponent]
public sealed class PlayerInputRouter : MonoBehaviour
{
    [Header("Actions")]
    [SerializeField] private InputActionAsset actions;
    [SerializeField] private string actionMapName = "Player";

    [Header("Targets")]
    [SerializeField] private CharacterCustomController characterController;
    [SerializeField] private WeaponSwitcher weaponSwitcher;
    [SerializeField, Tooltip("Optional fixed weapon command source used when no WeaponSwitcher is assigned.")]
    private MonoBehaviour weaponCommandSource;

    [Header("Behaviour")]
    [SerializeField, Tooltip("Treat Look as an absolute screen position for pointer devices.")]
    private bool pointerLookIsScreenPosition = true;

    private readonly PlayerInputState state = new PlayerInputState();

    private InputActionMap actionMap;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction sprintAction;
    private InputAction fireAction;
    private InputAction reloadAction;
    private InputAction switchFireModeAction;
    private InputAction weapon1Action;
    private InputAction weapon2Action;
    private InputAction hideWeaponAction;

    private IWeaponCommands fixedWeaponCommands;
    private IWeaponInputOwnership fixedInputOwnership;
    private WeaponController previousActiveWeapon;
    private bool subscribed;

    public bool IsReady => actionMap != null;
    public Vector2 MovementInput => state.Move;
    public bool FireHeld => state.FireHeld;

    private void Awake()
    {
        ResolveTargets();
        ResolveActions();
    }

    private void OnEnable()
    {
        ResolveTargets();
        ResolveActions();
        Subscribe();
        actionMap?.Enable();
        previousActiveWeapon = weaponSwitcher != null ? weaponSwitcher.ActiveWeaponController : null;
        SetExternalInputOwnership(true);
    }

    private void OnDisable()
    {
        if (state.ReleaseFire())
        {
            StopCurrentWeaponFire();
        }

        SetExternalInputOwnership(false);
        actionMap?.Disable();
        Unsubscribe();
        previousActiveWeapon = null;
        state.Reset();
        ApplyContinuousState();
    }

    private void Update()
    {
        if (actionMap == null)
        {
            return;
        }

        state.SetMove(ReadMovementInput());
        state.SetSprint((sprintAction != null && sprintAction.IsPressed()) || IsKeyboardSprintPressed());

        if (lookAction != null)
        {
            bool isPointer = lookAction.activeControl?.device is Pointer;
            state.SetLook(
                lookAction.ReadValue<Vector2>(),
                pointerLookIsScreenPosition && isPointer);
        }

        ApplyContinuousState();
    }

    private Vector2 ReadMovementInput()
    {
        Vector2 input = moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        if (input.sqrMagnitude > 0.0001f || Keyboard.current == null)
        {
            return input;
        }

        // Keep keyboard movement functional even when an editor/device control
        // scheme does not resolve the composite action correctly.
        float x = (Keyboard.current.dKey.isPressed ? 1f : 0f) - (Keyboard.current.aKey.isPressed ? 1f : 0f);
        float y = (Keyboard.current.wKey.isPressed ? 1f : 0f) - (Keyboard.current.sKey.isPressed ? 1f : 0f);
        return Vector2.ClampMagnitude(new Vector2(x, y), 1f);
    }

    private bool IsKeyboardSprintPressed() => Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

    private void ApplyContinuousState()
    {
        characterController?.SetMovementInput(state.Move);
        characterController?.SetSprintRequested(state.Sprint);
        characterController?.SetLookInput(state.Look, state.LookIsScreenPosition);
    }

    private void ResolveTargets()
    {
        characterController ??= GetComponent<CharacterCustomController>();
        weaponSwitcher ??= GetComponent<WeaponSwitcher>();
        fixedWeaponCommands = weaponCommandSource as IWeaponCommands;
        fixedInputOwnership = weaponCommandSource as IWeaponInputOwnership;
    }

    private void ResolveActions()
    {
        if (actions == null)
        {
            Debug.LogError($"{nameof(PlayerInputRouter)} requires an InputActionAsset.", this);
            return;
        }

        actionMap = actions.FindActionMap(actionMapName, throwIfNotFound: false);
        if (actionMap == null)
        {
            Debug.LogError($"Input action map '{actionMapName}' was not found.", this);
            return;
        }

        moveAction = FindAction("Move");
        lookAction = FindAction("Look");
        sprintAction = FindAction("Sprint");
        fireAction = FindAction("Fire");
        reloadAction = FindAction("Reload");
        switchFireModeAction = FindAction("SwitchFireMode");
        weapon1Action = FindAction("Weapon1");
        weapon2Action = FindAction("Weapon2");
        hideWeaponAction = FindAction("HideWeapon");
    }

    private InputAction FindAction(string actionName)
    {
        InputAction action = actionMap.FindAction(actionName, throwIfNotFound: false);
        if (action == null)
        {
            Debug.LogError($"Input action '{actionMapName}/{actionName}' was not found.", this);
        }

        return action;
    }

    private void Subscribe()
    {
        if (subscribed)
        {
            return;
        }

        if (fireAction != null)
        {
            fireAction.started += OnFireStarted;
            fireAction.canceled += OnFireCanceled;
        }

        if (reloadAction != null) reloadAction.performed += OnReload;
        if (switchFireModeAction != null) switchFireModeAction.performed += OnSwitchFireMode;
        if (weapon1Action != null) weapon1Action.performed += OnWeapon1;
        if (weapon2Action != null) weapon2Action.performed += OnWeapon2;
        if (hideWeaponAction != null) hideWeaponAction.performed += OnHideWeapon;
        if (weaponSwitcher != null) weaponSwitcher.ActiveWeaponChanged += OnActiveWeaponChanged;
        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed)
        {
            return;
        }

        if (fireAction != null)
        {
            fireAction.started -= OnFireStarted;
            fireAction.canceled -= OnFireCanceled;
        }

        if (reloadAction != null) reloadAction.performed -= OnReload;
        if (switchFireModeAction != null) switchFireModeAction.performed -= OnSwitchFireMode;
        if (weapon1Action != null) weapon1Action.performed -= OnWeapon1;
        if (weapon2Action != null) weapon2Action.performed -= OnWeapon2;
        if (hideWeaponAction != null) hideWeaponAction.performed -= OnHideWeapon;
        if (weaponSwitcher != null) weaponSwitcher.ActiveWeaponChanged -= OnActiveWeaponChanged;
        subscribed = false;
    }

    private IWeaponCommands CurrentWeaponCommands =>
        weaponSwitcher != null && weaponSwitcher.ActiveWeaponController != null
            ? weaponSwitcher.ActiveWeaponController
            : fixedWeaponCommands;

    private IWeaponInputOwnership CurrentInputOwnership =>
        weaponSwitcher != null && weaponSwitcher.ActiveWeaponController != null
            ? weaponSwitcher.ActiveWeaponController
            : fixedInputOwnership;

    private void SetExternalInputOwnership(bool enabled) =>
        CurrentInputOwnership?.SetExternalInputEnabled(enabled);

    private void StopCurrentWeaponFire() => CurrentWeaponCommands?.StopFire();

    private void OnFireStarted(InputAction.CallbackContext _)
    {
        if (state.PressFire())
        {
            CurrentWeaponCommands?.StartFire();
        }
    }

    private void OnFireCanceled(InputAction.CallbackContext _)
    {
        if (state.ReleaseFire())
        {
            CurrentWeaponCommands?.StopFire();
        }
    }

    private void OnReload(InputAction.CallbackContext _) => CurrentWeaponCommands?.Reload();
    private void OnSwitchFireMode(InputAction.CallbackContext _) => CurrentWeaponCommands?.SwitchFireMode();
    private void OnWeapon1(InputAction.CallbackContext _) => weaponSwitcher?.RequestSwitch(0);
    private void OnWeapon2(InputAction.CallbackContext _) => weaponSwitcher?.RequestSwitch(1);
    private void OnHideWeapon(InputAction.CallbackContext _) => weaponSwitcher?.HideAllWeapons();

    private void OnActiveWeaponChanged(WeaponController current)
    {
        if (previousActiveWeapon != current)
        {
            previousActiveWeapon?.StopFire();
            previousActiveWeapon?.SetExternalInputEnabled(false);
        }

        current?.SetExternalInputEnabled(isActiveAndEnabled);
        previousActiveWeapon = current;
    }

    private void OnValidate()
    {
        actionMapName = string.IsNullOrWhiteSpace(actionMapName) ? "Player" : actionMapName.Trim();
    }
}
