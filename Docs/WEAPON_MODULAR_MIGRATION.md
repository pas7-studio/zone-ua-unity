# Weapon modular component migration

Target editor: Unity 6 / 6000.5.5f1.

This stacked change integrates the modular combat components introduced by the baseline branch into `WeaponController` while preserving legacy prefab compatibility.

## Optional modules

A weapon prefab may add the following components on the same GameObject as `WeaponController`:

- `ProjectileSpawner`;
- `ShellEjector`;
- `WeaponAudio`;
- `WeaponRecoil`;
- `RuntimeObjectSpawnerAdapter` for pool-backed spawning.

`WeaponController` resolves these components automatically in `Awake`. Explicit serialized references remain available when a module lives elsewhere.

## Migration order for one prefab

1. Add `RuntimeObjectSpawnerAdapter`.
2. Add `ProjectileSpawner`, assign the muzzle and adapter.
3. Add `ShellEjector`, assign the ejection point, shell prefab and adapter.
4. Add `WeaponAudio`; its AudioSource is resolved automatically.
5. Add `WeaponRecoil` and tune fallback values only when no `WeaponDefinition` is assigned.
6. Enter Play Mode and verify single, burst, automatic, reload, empty-magazine, projectile and shell behaviour.
7. Only after verification remove duplicated legacy values from that prefab.

## Ammo HUD migration

Add `WeaponAmmoPresenter` to the player HUD or another suitable scene object:

1. Assign the player's `WeaponSwitcher`.
2. Assign the existing `UIAmmoSystem` view.
3. Enable `hideWhenUnarmed` when hiding all weapons should also hide the ammo HUD.

`WeaponSwitcher` exposes `ActiveWeaponChanged` and no longer accesses `GlobalSystem.AmmoUI`. `WeaponAmmoPresenter` binds to the active `WeaponController`, subscribes to `AmmoChanged`, refreshes immediately when a weapon becomes active and removes subscriptions when switching or disabling.

The direct UI bridge inside `WeaponController` remains temporarily as a compatibility fallback until all production scenes have a configured presenter.

## Compatibility behaviour

When a module is absent, `WeaponController` keeps using its existing serialized fields and legacy implementation. Existing prefabs therefore do not require a simultaneous migration.

When a `WeaponDefinition` and `ProjectileDefinition` are assigned, shared definition values take priority over legacy fallback values.

## Validation checklist

- No Console compilation errors in Unity 6000.5.5f1.
- Existing weapon prefabs still work without new modules.
- A migrated prefab uses the assigned modules exactly once per shot.
- Ammo is consumed only after projectile spawn succeeds.
- Empty-magazine sound does not consume ammo.
- Reload cancels firing and burst state.
- Projectile and shell instances return through the runtime pool when an adapter is assigned.
- Weapon switching resets recoil and transient fire state.
- Ammo HUD updates from the active weapon only.
- Switching weapons does not leave duplicate ammo subscriptions.
- Hiding all weapons hides or clears the ammo HUD according to presenter settings.
