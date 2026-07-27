# Inventory, Items and Construction

This module adds definition-driven items, deterministic inventory state, persistent pickup/drop objects and persistent construction progress.

## Assets

Create item assets with:

`Create -> Zone UA -> Inventory -> Item Definition`

Each item requires a unique `Item Id`. `World Prefab` is used when dropping the item. `Persistent World Prefab Id` must match an entry in `PersistentPrefabCatalog`; when empty it falls back to the item ID.

Create a catalog with:

`Create -> Zone UA -> Inventory -> Item Catalog`

Create buildable definitions with:

`Create -> Zone UA -> Construction -> Definition`

A construction definition contains a stable ID, required work, material costs and the completed prefab reference.

## Player composition

Add these components to the persistent player root:

- `PersistentIdentity`
- `InventoryComponent`
- `InventorySaveParticipant`
- `InventoryDropper`

Assign the `ItemCatalog` to `InventoryDropper`. Capacity `0` means unlimited capacity.

## World pickup prefab

A dropped or authored pickup should contain:

- `PersistentIdentity`
- `TransformSaveParticipant`
- `WorldItemPickup`
- a trigger collider

Register runtime pickup prefabs in `PersistentPrefabCatalog`. `InventoryDropper` assigns a new logical object ID and configures the dropped stack amount. `WorldItemPickup` persists its amount through participant key `world-item`.

When a pickup succeeds it calls `PersistentIdentity.MarkDestroyed()`, creating a tombstone. The item therefore does not reappear after save/load.

## Construction composition

A persistent construction object should contain:

- `PersistentIdentity`
- `TransformSaveParticipant`
- `ConstructionSite`

`ConstructionSite` is itself a save participant with key `construction`. It stores required work, applied work and whether resources were committed.

Typical flow:

1. Place the construction-site prefab.
2. Call `CommitResources(playerInventory)` once.
3. Call `ApplyWork(amount)` from worker or interaction logic.
4. Swap `Incomplete Root` and `Completed Root` when progress reaches 100%.
5. Save/load restores the exact construction progress.

Resource consumption is atomic. If any required material is missing, no material is removed.

## Validation

Run:

`Zone UA -> Validation -> Validate Inventory and Construction`

The validator reports:

- empty or duplicate item IDs;
- empty or duplicate construction IDs;
- invalid construction costs;
- item definitions without world prefabs;
- loaded inventories without `InventorySaveParticipant`;
- pickups or construction sites without `PersistentIdentity`.

Errors block Unity builds. Missing optional presentation references remain warnings.

## Required Play Mode checks

- collect a world pickup and confirm inventory count changes;
- save, reload and confirm the collected object remains absent;
- drop a stack and confirm a new stable object ID is assigned;
- save, reload and confirm the dropped stack and amount restore once;
- verify full inventory prevents pickup without creating a tombstone;
- commit construction resources and verify exact atomic deduction;
- partially build, save and reload;
- complete construction and verify the completed visual persists;
- validate duplicate item/construction IDs are blocked.
