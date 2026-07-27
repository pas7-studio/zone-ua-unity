# Input System Migration

## Overview

The player input layer now uses Unity Input System through one `PlayerInputRouter` and the `ZoneUAInput.inputactions` asset.

Gameplay components no longer need to poll keyboard or mouse state directly:

- `CharacterCustomController` receives movement, sprint and look commands.
- `WeaponSwitcher` receives explicit switch/hide commands.
- `WeaponInput` is a callback-only compatibility bridge.
- `WeaponController` is driven through `IWeaponCommands` and `IWeaponInputOwnership`.

## Unity version and package

The target project is Unity `6000.5.5f1`.

The manifest references:

```text
com.unity.inputsystem 1.17.0
```

After opening the branch in Unity, allow Package Manager to regenerate `Packages/packages-lock.json` for the migrated Unity project.

## Scene setup

Add `PlayerInputRouter` to the player root.

Assign:

```text
Actions              Assets/_ZoneUA/Input/ZoneUAInput.inputactions
Action Map Name      Player
Character Controller player CharacterCustomController
Weapon Switcher      player WeaponSwitcher
```

`Weapon Command Source` is only needed when the actor has one fixed weapon and no `WeaponSwitcher`.

## Input actions

The `Player` map contains:

| Action | Keyboard/mouse | Gamepad |
|---|---|---|
| Move | WASD | Left stick |
| Look | Pointer position | Right stick |
| Sprint | Left Shift | Left-stick press |
| Fire | Left mouse | Right trigger |
| Reload | R | West button |
| SwitchFireMode | B | D-pad up |
| Weapon1 | 1 | D-pad left |
| Weapon2 | 2 | D-pad right |
| HideWeapon | Q | North button |

Bindings can be edited in the Input Actions editor without changing gameplay scripts.

## Active Input Handling

After opening the project in Unity 6, set:

```text
Project Settings → Player → Active Input Handling → Input System Package (New)
```

Use `Both` only during short migration testing if an unconverted third-party asset still requires the legacy Input Manager.

## Prefab migration

### Player

1. Add `PlayerInputRouter`.
2. Assign the actions asset and target components.
3. Remove or disable duplicate input scripts.
4. Verify the player has exactly one enabled input router.

### Weapon prefabs

`WeaponInput` no longer polls `Input.GetButton` or key codes. It exposes callback methods only:

```text
StartFire
StopFire
Reload
SwitchFireMode
```

Existing `WeaponController` command methods remain compatible with AI and scripted control.

### Weapon switching

`WeaponSwitcher` no longer reads Alpha1, Alpha2 or Q in `Update`. Input is delivered through `RequestSwitch(index)` and `HideAllWeapons()`.

## Ownership rules

When the router is enabled it calls:

```text
IWeaponInputOwnership.SetExternalInputEnabled(true)
```

When the active weapon changes:

- the previous weapon is stopped;
- previous external ownership is disabled;
- the new weapon receives external ownership.

Disabling the router clears movement, sprint and held-fire state.

## Validation checklist

- [ ] Unity resolves Input System without package errors.
- [ ] `Active Input Handling` is set to the new Input System.
- [ ] there is one enabled `PlayerInputRouter` on the player.
- [ ] WASD and left stick move with normalized diagonal speed.
- [ ] sprint starts and stops correctly.
- [ ] pointer look preserves mouse-facing behaviour.
- [ ] gamepad look changes facing correctly.
- [ ] fire starts once and stops on cancel.
- [ ] disabling input while firing stops the weapon.
- [ ] reload and fire-mode switching execute once per press.
- [ ] weapon 1, weapon 2 and hide actions work.
- [ ] changing weapons cannot leave the previous weapon firing.
- [ ] player death disables movement and clears input.
- [ ] NPC weapons remain controlled by AI commands.
- [ ] `PlayerInputStateTests` pass in EditMode.
