# Resource Gathering, Jobs and Production

This module adds deterministic economy loops on top of inventory, construction and stable-ID persistence.

## Content definitions

Create assets through:

- `Create -> Zone UA -> Economy -> Resource Node Definition`
- `Create -> Zone UA -> Economy -> Production Recipe`

Resource definitions require a unique resource ID, yielded item, total units, units per harvest and work per harvest. Optional respawn time restores the node after depletion.

Recipes require a unique recipe ID, duration, valid inputs and at least one output.

## Resource node prefab

Recommended composition:

- `PersistentIdentity`
- `TransformSaveParticipant`
- `ResourceNode`
- collider used by selection or worker interaction
- optional available/depleted presentation roots

`ResourceNode` persists remaining units, partial accumulated work and respawn time through participant key `resource-node`.

Harvest output is committed to an `InventoryComponent`. When the destination inventory cannot accept the harvested batch, the node restores the removed units, so resources are never silently lost.

## Worker job agent

Add `WorkerJobAgent` and an `InventoryComponent` to a worker prefab. Jobs are assigned through:

```csharp
worker.AssignHarvest(resourceNode, storageInventory);
worker.ClearJob();
```

The current implementation deliberately owns job state and work execution, but does not replace the existing movement/pathfinding system. The unit command layer should move the worker into `interactionRange`; once in range, the agent applies deterministic work.

Worker job state is persisted through participant key `worker-job`. After load, target IDs remain available for a higher-level job registry to rebind scene references.

## Production facility

Recommended composition:

- `PersistentIdentity`
- `TransformSaveParticipant`
- one or two `InventoryComponent` instances
- `InventorySaveParticipant` for every persistent inventory
- `ProductionFacility`

A facility consumes all required inputs atomically when a recipe is queued. It then advances production over time. If output storage is full, progress pauses before completing the cycle, preventing lost output.

The queue stores recipe IDs, remaining cycles and current elapsed time through participant key `production-facility`.

## Validation

Run:

`Zone UA -> Validation -> Validate Economy Content`

Validation blocks builds for:

- empty resource IDs;
- duplicate resource IDs;
- resource definitions without yielded items;
- empty recipe IDs;
- duplicate recipe IDs;
- recipes without outputs;
- invalid input or output entries.

## Required Play Mode checks

1. Assign a worker to a resource node and move it into interaction range.
2. Confirm partial work accumulates deterministically.
3. Confirm a full carry inventory does not delete resource units.
4. Save during partial harvest and reload.
5. Deplete and respawn a renewable node.
6. Queue multiple production cycles.
7. Save during an active production cycle and reload.
8. Fill output storage and confirm production pauses.
9. Free storage capacity and confirm production resumes exactly once.
10. Unload and reload scenes and verify stable-ID participants restore without duplicates.
