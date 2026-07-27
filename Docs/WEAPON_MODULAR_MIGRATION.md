# Weapon modular component migration

Target editor: Unity 6 / 6000.5.5f1.

This stacked change completes the modular weapon runtime introduced by the baseline branch while preserving legacy prefab compatibility.

## Runtime architecture

`WeaponController` is now an orchestrator around focused modules and deterministic state objects:

- `ProjectileSpawner` creates projectiles and applies initial velocity;
- `ShellEjector` creates and releases shell objects;
- `WeaponAudio` owns shot, reload and empty-magazine audio;
- `WeaponRecoil` owns recoil/spread state;
- `WeaponFireState` owns trigger, cadence, burst count and burst cooldown;
- `WeaponReloadState` owns reload start, completion and cancellation;
- `WeaponAmmoPresenter` binds the active weapon to `UIAmmoSystem` through events;
- `RuntimeObjectSpawnerAdapter` bridges modular components to `GlobalSystem` pooling.

`WeaponFireState` and `WeaponReloadState` do not access `MonoBehaviour`, coroutines or `Time`. The controller passes the current time explicitly, making their behaviour deterministic and EditMode-testable.

## Optional prefab modules

A weapon prefab may add the following components on the same GameObject as `WeaponController`:

- `ProjectileSpawner`;
- `ShellEjector`;
- `WeaponAudio`;
- `WeaponRecoil`;
- `RuntimeObjectSpawnerAdapter` for pool-backed spawning.

`WeaponController` resolves these components automatically in `Awake`. Explicit serialized references remain available when a module lives elsewhere. When a module is absent, the corresponding legacy serialized implementation remains available as a temporary prefab fallback.

## Migration order for one weapon prefab

1. Add `RuntimeObjectSpawnerAdapter`.
2. Add `ProjectileSpawner`, assign the muzzle and adapter.
3. Add `ShellEjector`, assign the ejection point, shell prefab and adapter.
4. Add `WeaponAudio`; its `AudioSource` is resolved automatically.
5. Add `WeaponRecoil` and tune fallback values only when no `WeaponDefinition` is assigned.
6. Assign `WeaponDefinition` and `ProjectileDefinition` assets where available.
7. Enter Play Mode and verify single, burst, automatic, reload, empty-magazine, projectile and shell behaviour.
8. After validation, clear duplicated legacy prefab values in a separate Unity-authored prefab migration commit.

Do not remove legacy serialized fields from the C# component until all production prefabs have been migrated and saved by Unity, because those fields currently preserve old prefab data.

## Ammo HUD migration

Add `WeaponAmmoPresenter` to the player HUD or another suitable scene object:

1. Assign the player's `WeaponSwitcher`.
2. Assign the existing `UIAmmoSystem` view.
3. Enable `hideWhenUnarmed` when hiding all weapons should also hide the ammo HUD.

`WeaponSwitcher` exposes `ActiveWeaponChanged` and does not access `GlobalSystem.AmmoUI`. `WeaponAmmoPresenter` binds only to the active `WeaponController`, subscribes to `AmmoChanged`, refreshes immediately and removes subscriptions when switching or disabling.

`WeaponController` no longer references `UIAmmoSystem`; gameplay-to-HUD communication is event-driven.

## Automated tests

EditMode tests are located in:

`Assets/_ZoneUA/Tests/EditMode/WeaponStateTests.cs`

They cover:

- one shot per single-fire trigger press;
- automatic-fire cadence;
- burst shot count;
- burst cooldown;
- failed-shot burst cancellation;
- reload completion timing;
- invalid reload starts;
- fire/reload reset during weapon changes.

Run them from Unity Test Runner in EditMode after the Unity 6 migration files are present.

## Validation checklist

- No Console compilation errors in Unity 6000.5.5f1.
- All `WeaponStateTests` pass in EditMode.
- Existing weapon prefabs still work without modular components.
- A migrated prefab uses each assigned module exactly once per successful shot.
- Ammo is consumed only after projectile spawn succeeds.
- Empty-magazine sound does not consume ammo.
- Single fire requires a new trigger press for each shot.
- Automatic fire respects `ShotInterval`.
- Burst fire respects `BurstSize` and `BurstCooldown`.
- Reload cancels trigger and burst state and completes once.
- Projectile and shell instances return through the runtime pool when an adapter is assigned.
- Weapon switching resets fire, reload and recoil state.
- Ammo HUD updates from the active weapon only.
- Switching weapons does not leave duplicate HUD subscriptions.
- Hiding all weapons hides or clears the ammo HUD according to presenter settings.
- Player and NPC weapon paths both pass Play Mode smoke tests.
