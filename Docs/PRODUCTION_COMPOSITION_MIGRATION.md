# Production prefab and scene composition migration

This module applies the architecture refactors to real Unity prefabs and loaded scenes through Unity Editor APIs. It intentionally does not edit prefab or scene YAML outside Unity.

## Why the migration runs inside Unity

Prefab assets may contain nested prefab links, overrides, managed references and GUID-backed object references. `PrefabUtility.LoadPrefabContents` and `PrefabUtility.SaveAsPrefabAsset` preserve those relationships, whereas direct text replacement can corrupt them.

## Menu commands

Open Unity and use:

- `Zone UA/Migration/Audit Production Composition`
- `Zone UA/Migration/Migrate Selected Prefabs`
- `Zone UA/Migration/Migrate All Production Prefabs`
- `Zone UA/Migration/Audit Open Scenes`
- `Zone UA/Migration/Migrate Open Scenes`

Always run the audit first and commit the repository before running a bulk migration.

## Composition rules

### Player root

An object containing `CharacterCustomController` must also contain:

- `Health`
- `Death`
- `PlayerInputRouter`
- `WeaponSwitcher`

These components are safe to add automatically, but their serialized references still need Inspector review.

### NPC actor

An object containing `NPCController` must also contain:

- `Health`
- `Death`
- `FactionMember`

### Weapon

An object containing `WeaponController` must also contain:

- `Weapon`
- `ProjectileSpawner`
- `ShellEjector`
- `WeaponAudio`
- `WeaponRecoil`

The migrator adds missing components but does not guess muzzle transforms, shell points, audio clips, definitions or prefab references.

### Damage presentation

Objects containing `Health` are audited for `DamageEffectsPresenter`. This is report-only because destructibles and actors may need different effect policies and references.

### World root

An object containing `MapGenerator` must also contain `ChunkManager`.

### Ammo HUD

Objects containing `UIAmmoSystem` are audited for `WeaponAmmoPresenter`. This is report-only because the presenter requires explicit references to the active `WeaponSwitcher` and ammo view.

## Safe workflow

1. Open the project in Unity 6000.5.5f1.
2. Allow package import and script compilation to finish.
3. Run `Zone UA/Validation/Validate Project`.
4. Run `Audit Production Composition`.
5. Migrate a small representative prefab set first.
6. Review every added component and assign definitions, transforms, prefabs, masks and views.
7. Run EditMode tests.
8. Open production scenes and run `Audit Open Scenes`.
9. Run `Migrate Open Scenes`, review the diff and save scenes manually.
10. Perform player, NPC, weapon, HUD and world-generation smoke tests.

## Required Inspector review after automatic migration

### Player

- assign `ZoneUAInput.inputactions` to `PlayerInputRouter`
- assign `CharacterCustomController`
- assign `WeaponSwitcher`
- verify `Health`, `Death` and damage presentation

### NPC

- assign `NpcDefinition`
- assign `FactionDefinition`
- verify detection and line-of-sight masks
- verify weapon prefab and patrol points

### Weapon

- assign `WeaponDefinition`
- configure projectile muzzle
- configure shell ejection point
- configure audio source and clips
- configure recoil values or definition values
- verify runtime spawner adapter

### HUD

- assign `WeaponSwitcher`
- assign `UIAmmoSystem`
- verify unarmed visibility behaviour

### World

- assign `WorldGenerationSettings`
- assign generation root
- point `ChunkManager` to the same generated root
- validate biome assets

## Limitations

The tool only adds components that are unambiguously safe. It never guesses scene object references or content assets. Report-only rules remain unresolved until a designer wires them in the Inspector.

The tool has not been executed in this environment. Unity compilation, prefab saving and scene migration must be run locally in Unity 6000.5.5f1.
