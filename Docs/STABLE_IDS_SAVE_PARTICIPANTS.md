# Stable IDs and save participants

This module extends schema version 3 with mutable world-object persistence.

## Identity model

Every persistent logical object uses `PersistentIdentity`.

Scene objects receive a unique `objectId` stored in the scene. Runtime-spawned objects receive:

- a unique runtime `objectId` for that individual instance;
- a stable `prefabId` used to recreate the correct prefab;
- `runtimeSpawned = true`.

Do not assign one shared object ID to a prefab asset and reuse it for every instance. IDs identify logical instances, while prefab IDs identify content templates.

## Save participants

Components implement:

```csharp
IPersistentSaveParticipant
```

Each participant provides:

- a stable `ParticipantKey` unique within one object;
- a participant payload version;
- JSON-compatible capture output;
- restore logic capable of handling its supported versions.

Included participants:

- `TransformSaveParticipant`;
- `HealthSaveParticipant`.

Additional participants should be small and domain-specific, for example:

- building construction state;
- container inventory;
- dropped-item stack and durability;
- NPC faction, alert and schedule state;
- door or switch state.

Do not put all mutable state in one universal component.

## Tombstones

Calling:

```csharp
persistentIdentity.MarkDestroyed();
```

adds the object ID to `PersistentTombstoneRegistry` and disables the object.

Tombstones prevent authored scene objects from reappearing after loading a save in which they were destroyed, collected or permanently removed.

Do not create tombstones from `OnDestroy`. Scene unloading and application shutdown are not gameplay destruction.

## Runtime prefab catalog

Create:

```text
Create -> Zone UA -> Persistence -> Persistent Prefab Catalog
```

Register every runtime-spawned persistent prefab with a unique prefab ID. `SaveGameCoordinator` uses the catalog to recreate missing runtime instances before participant payloads are restored.

Each catalog prefab should include:

- `PersistentIdentity`;
- `TransformSaveParticipant` when position matters;
- domain participants such as health or inventory;
- no preassigned runtime object ID.

## Scene workflow

1. Add `PersistentIdentity` to mutable authored scene objects.
2. Add the required save participants.
3. Run `Zone UA -> Persistence -> Assign Missing Stable IDs`.
4. Review the scene diff and save the scene.
5. Run `Zone UA -> Persistence -> Validate Stable IDs`.
6. Create and assign `PersistentPrefabCatalog` on `SaveGameCoordinator`.
7. Assign an optional `Runtime Persistent Root`.
8. Test save, mutation, scene reload and restore.

## Validation rules

Build validation fails when loaded production scenes contain:

- a scene persistent object without an ID;
- duplicate object IDs;
- a runtime identity without a prefab ID;
- duplicate participant keys on one object.

The prefab catalog separately reports empty IDs, duplicates and missing prefab references through `ValidateEntries()` and Inspector review.

## Save order

A save captures:

1. player and world seed;
2. all active and inactive `PersistentIdentity` objects;
3. participant payloads ordered by participant key;
4. tombstones ordered by object ID;
5. the complete versioned snapshot through atomic save storage.

A load performs:

1. scene transition when required;
2. world seed assignment;
3. tombstone registry replacement;
4. recreation of missing runtime-spawned objects;
5. participant restore;
6. player transform, health and active weapon restore.

## Required Unity validation

- Project compiles in Unity 6000.5.5f1.
- `PersistentWorldStateTests` pass.
- Duplicate scene IDs are reported.
- Duplicate participant keys are reported.
- A destroyed scene object stays absent after save/load.
- A runtime dropped item is recreated from the catalog.
- Transform and health restore correctly.
- Scene unload does not create tombstones.
- Saving twice does not duplicate runtime instances.
- Missing prefab IDs produce warnings without corrupting the remaining restore.

## Current boundary

This module provides identity, capture, tombstones and runtime recreation. Complex inventories and NPC simulation state still need their own participant implementations. Save payloads should reference stable definition IDs rather than direct Unity object references.
