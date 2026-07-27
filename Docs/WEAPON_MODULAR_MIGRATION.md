# Weapon modular component migration

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

The legacy UI bridge remains temporarily in `WeaponController`; it will be removed after the HUD subscribes to `IWeaponCommands` events.
