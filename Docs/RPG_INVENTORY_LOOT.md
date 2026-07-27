# RPG Inventory and Persistent Loot

This module provides definition-driven items, deterministic inventories, persistent world pickups, searchable containers and corpse loot for the Zone UA top-down shooter RPG.

## Item assets

Create items with:

`Create -> Zone UA -> Inventory -> Item Definition`

Each item requires a unique `Item Id`. Configure category, base loot value, weight, stack size, world prefab and persistent world prefab ID. The value/category fields are intended for future NPC loot evaluation.

Create an `ItemCatalog` and assign it to systems that need to resolve item IDs into definitions or world prefabs.

## Player and NPC inventories

A persistent actor should contain:

- `PersistentIdentity`
- `InventoryComponent`
- `InventorySaveParticipant`

Both players and raid NPCs use the same inventory state. Initial spawn equipment can be configured through `InventoryComponent.initialItems`; acquired loot is added through normal transfer operations.

Inventory transfer is atomic. A failed transfer caused by missing source items or destination capacity leaves both inventories unchanged.

## World pickups

A persistent world item should contain:

- `PersistentIdentity`
- `TransformSaveParticipant`
- `WorldItemPickup`
- an interaction or trigger collider

`WorldItemPickup` implements `ILootSource`, so the player and AI can use the same loot contract. A successful pickup creates a tombstone through `PersistentIdentity.MarkDestroyed()`, preventing the authored or runtime item from reappearing after save/load.

Dropped item prefabs must be registered in `PersistentPrefabCatalog` using the item's persistent world prefab ID.

## Searchable containers

A crate, stash or other searchable container should contain:

- `PersistentIdentity`
- `TransformSaveParticipant`
- `InventoryComponent`
- `InventorySaveParticipant`
- `LootContainer`

`LootContainer` stores whether the source is unsearched, searched or empty. Container contents are persisted by `InventorySaveParticipant`; search state is persisted through participant key `loot-source`.

The configured search duration is presentation/gameplay data for the interaction and future raid-NPC task executor. The container does not own movement or UI.

## Corpse loot

A corpse prefab should contain the normal loot-container composition plus `CorpseLootContainer`.

On death, initialise it from the dead actor's inventory:

`corpseLootContainer.InitialiseFrom(deadActorInventory)`

The corpse receives an inventory snapshot containing both spawn equipment and all acquired loot. By default the source inventory is cleared so pooled or disabled actors cannot expose the same items twice.

## Loot reservations

`LootReservationRegistry` prevents multiple raid NPCs from selecting the same world item, crate or corpse. Reservations contain a source ID, owner NPC ID and expiry time.

Reservations are runtime coordination state and should normally not be written into save files. After loading, NPCs re-evaluate available loot and reserve targets again.

Release reservations when an NPC dies, changes goal, cannot reach the target, completes looting or exceeds the reservation timeout.

## Validation

Run:

`Zone UA -> Validation -> Validate Inventory and Loot`

The validator checks:

- empty or duplicate item IDs;
- missing world prefabs and zero-value item warnings;
- inventories without `InventorySaveParticipant`;
- world pickups without `PersistentIdentity`;
- loot containers without identity, inventory or inventory persistence;
- corpse containers without `LootContainer`.

Errors block Unity builds.

## Required Play Mode checks

- collect a world pickup and confirm inventory count changes;
- confirm a full inventory does not consume or tombstone the item;
- drop a stack and confirm a new stable object ID is assigned;
- save/load a dropped stack exactly once;
- search a crate and transfer selected items;
- verify destination capacity cannot delete source loot;
- kill an NPC and initialise a corpse from its complete inventory;
- save/load corpse contents and search state;
- reserve one loot source from two NPCs and verify only one succeeds;
- expire or release a reservation and verify another NPC can claim it.
