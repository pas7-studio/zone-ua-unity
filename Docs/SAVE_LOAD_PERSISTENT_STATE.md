# Save/load and persistent state

This module introduces a versioned save format and a single runtime coordinator for player-facing persistence.

## Stored state

`SaveGameData` currently stores:

- schema version;
- save identifier and UTC timestamp;
- active gameplay scene;
- world seed;
- player position and Z rotation;
- current and maximum health;
- active weapon slot.

The schema is intentionally explicit. New persistent fields must be added with a schema-version increment and a migration step.

## Storage guarantees

`SaveSlotStore` writes under:

`Application.persistentDataPath/Saves`

Each slot uses:

- `<slot>.json` for the primary save;
- `<slot>.json.bak` for the previous valid save;
- `<slot>.json.tmp` during atomic replacement.

The payload is wrapped in an envelope with a SHA-256 checksum. Loading tries the primary file first and falls back to the backup if the primary file is missing, truncated, malformed or fails checksum validation.

## Bootstrap setup

Add `SaveGameCoordinator` to the persistent Bootstrap root beside:

- `GlobalSystem`;
- `SceneBootstrapper`.

Assign:

- player root;
- player `Health`;
- player `WeaponSwitcher`;
- `SceneBootstrapper`;
- default slot;
- autosave interval.

The coordinator can resolve player references from the configured player root. For production, explicit Inspector references are preferred.

## Scene transitions

When a save references another gameplay scene, the coordinator asks `SceneBootstrapper` to load it. Player position, health and weapon slot are restored only after `SceneActivated` fires.

Overlapping scene transitions are rejected by the existing scene state machine. A failed transition produces `LoadFailed` and does not apply a partial snapshot.

## World seed integration

The world generator should call:

`SaveGameCoordinator.SetWorldSeed(resolvedSeed)`

immediately after resolving or generating the seed. During load, read `CurrentWorldSeed` before regenerating the world. The world must finish deterministic generation before player-dependent simulation resumes.

## Public API

- `SaveDefault()`
- `LoadDefault()`
- `Save(slotId)`
- `Load(slotId)`
- `Delete(slotId)`
- `SetWorldSeed(seed)`

Events:

- `Saved`
- `Loaded`
- `SaveFailed`
- `LoadFailed`

## Migration policy

`SaveGameMigrator` upgrades old snapshots one version at a time. Saves from a newer unsupported schema are rejected rather than loaded partially.

Never silently reinterpret a field. For renamed or structurally changed data, add an explicit migration and a test fixture representing the old schema.

## Required Unity validation

1. Open the project in Unity 6000.5.5f1.
2. Run all `SaveGamePersistenceTests`.
3. Add `SaveGameCoordinator` to Bootstrap.
4. Wire the production player references.
5. Save in Production, move and take damage, then load.
6. Save with weapon slot 2 active and verify restoration.
7. Save in another gameplay scene and verify additive transition before restore.
8. Corrupt the primary save and verify backup recovery.
9. Pause/resume on Android and verify pause autosave.
10. Verify quitting does not throw during Unity shutdown.
11. Verify the deterministic world seed is applied before player simulation.

## Not yet persisted

The current schema does not claim to persist every runtime object. NPC state, dropped items, constructed buildings, inventories and mutable world objects should be added through a separate stable-ID participant layer after their runtime ownership rules are finalised.
