# Damage, health and death migration

Target editor: Unity 6 / 6000.5.5f1.

This stacked change completes the damage, health and death runtime on top of `refactor/weapon-modular-components`.

## Runtime architecture

- `DamageInfo` is the single incoming damage payload.
- `DamageResolver` converts raw damage, resistance and a final multiplier into an integer applied amount.
- `HealthState` owns maximum health, current health, healing and lethal transitions without depending on scene objects.
- `DeathState` guarantees that death is entered only once.
- `Health` is the MonoBehaviour adapter that raises gameplay events and delegates presentation.
- `DamageEffectsPresenter` owns particles, decals and popup lifetime.
- `Death` disables movement, weapons, AI and optionally colliders/body simulation.
- `FactionDamagePolicy` is the deterministic friendly-fire decision used by `FactionMember`.

## Health setup

On every damageable character:

1. Keep or add `Health` and `Death`.
2. Set `defaultHeals` to the desired maximum health.
3. Optionally assign a `DamageResistanceProfile`.
4. Optionally tune `incomingDamageMultiplier`.
5. Add `DamageEffectsPresenter` when the character should spawn hit presentation.
6. Assign `DamageEffectSettings` and optionally a `RuntimeObjectSpawnerAdapter` to the presenter.

`Health` still exposes the legacy methods `receiveDamage`, `setHeals`, `restoreSomeHeals`, `restoreDefaultHeals`, `getHeals` and `getIsAlive` so existing UnityEvents can be migrated gradually.

## Damage resistance profile

Create one through:

`Assets > Create > Zone UA > Combat > Damage Resistance Profile`

Each entry maps a `DamageType` to a resistance in the range `-1..1`:

- `0` means unchanged damage;
- `0.25` means 25% reduction;
- `1` blocks that damage type;
- `-0.5` increases that damage type by 50%.

Duplicate damage-type entries are removed during validation.

## Damage effects

`DamageEffectsPresenter` reads `DamageEffectSettings` and is responsible for:

- one hit particle effect per applied hit;
- configured decal count and scatter radius;
- decal impulse and velocity attenuation;
- optional popup creation and lifetime;
- pool-backed spawning through `IRuntimeObjectSpawner` when assigned;
- `Instantiate`/`Destroy` fallback when no spawner is assigned.

Damage effects are not spawned for immune targets, fully resisted hits or zero damage.

## Death setup

`Death` automatically resolves common gameplay components on the same character:

- `CharacterCustomController`;
- `WeaponSwitcher`;
- child `WeaponController` components;
- `NPCController`;
- child `Collider2D` components;
- `Rigidbody2D` and `Animator` when present.

Use `behavioursToDisable` for additional character-specific scripts. `Dead()` is idempotent: animation, event dispatch and component shutdown happen once.

`disableBodySimulation` and `disableColliders` are configurable because some corpse prefabs may need physics or interaction after death.

## Events

`Health` exposes:

- `Damaged(DamageInfo)` after positive damage is applied;
- `DamageResolved(DamageInfo, DamageResolution)` after resistance calculation, including blocked hits;
- `HealthChanged(current, maximum)`;
- `Healed(restoredAmount)`;
- `Died()` exactly once.

`Death` exposes `DeathEntered()` exactly once.

## Validation checklist

- No Console compilation errors in Unity 6000.5.5f1.
- All EditMode tests pass.
- Raw projectile damage reaches `Health` through `DamageInfo`.
- Resistance values reduce, block or amplify the correct damage type.
- Immunity blocks damage and presentation.
- Healing clamps to maximum health and cannot revive a dead entity.
- Lethal damage sets health to zero and raises death once.
- Repeated damage after death does not replay animation or shutdown.
- Player input and all weapon controllers are disabled on death.
- NPC AI receives `PrepareForDeath` and is disabled.
- Ammo HUD hides through `WeaponSwitcher.ActiveWeaponChanged`, not through `Death` or `GlobalSystem`.
- Friendly fire follows the source faction profile.
- Damage particles, decals and popups use the pool when an adapter is configured.
- Legacy UnityEvents referencing old health methods still resolve.

The code scope is complete, but scene/prefab assignments and Play Mode validation must be performed in the Unity Editor.
