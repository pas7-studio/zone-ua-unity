# NPC state machine and faction migration

Target editor: Unity 6 / 6000.5.5f1.

## Runtime architecture

`NPCController` is now a scene adapter around deterministic AI runtime classes:

- `NpcBrainState` owns state transitions and target-memory timing;
- `NpcTargetScoring` ranks valid candidates;
- `FactionMember` decides whether a candidate can be damaged;
- `NPCController` performs Physics2D queries, movement, animation and weapon commands.

Supported states:

- `Idle`;
- `Patrol`;
- `Chase`;
- `Attack`;
- `Flee`;
- `Dead`.

`Dead` is terminal until the component is recreated or the brain is explicitly reset.

## Prefab setup

1. Keep `NPCController`, `Health`, `Death`, `Rigidbody2D` and `Animator` on the NPC root.
2. Add `FactionMember` and assign a `FactionDefinition`.
3. Assign an optional `NpcDefinition` to centralise movement, perception, combat and patrol tuning.
4. Configure `detectionLayers` so only targetable actor colliders are scanned.
5. Enable `requireLineOfSight` only when `lineOfSightBlockingLayers` is configured.
6. Assign patrol points in traversal order.
7. Keep the existing weapon prefab until weapon migration is validated.

## Faction behaviour

A candidate is considered targetable only when:

- it has a living `Health` component;
- it is not the NPC itself;
- `FactionMember.CanDamage(candidateFaction)` returns true;
- line of sight passes when enabled.

Tags are no longer used to determine hostility.

## State rules

- low health plus a visible target enters `Flee`;
- a visible target inside preferred attack distance enters `Attack`;
- a visible or recently lost target enters `Chase`;
- patrol points produce `Patrol` when no target is remembered;
- no patrol and no target produces `Idle`;
- death enters terminal `Dead` and stops movement and weapon fire.

## Validation checklist

- zero Console compilation errors;
- all `NpcBrainTests` pass in EditMode;
- same-faction targets are ignored unless friendly fire allows damage;
- neutral and friendly relations are ignored;
- hostile candidates are selected by nearest valid distance;
- dead targets are ignored;
- line-of-sight blocking works with the configured mask;
- losing a target keeps chase state for `loseTargetDelay`;
- low-health NPCs flee instead of firing;
- attack state fires or reloads through `WeaponController`;
- leaving attack stops firing;
- patrol points cycle without allocating coroutines;
- `PrepareForDeath` enters `Dead`, hides the weapon and stops movement;
- player and NPC death smoke tests still pass after the stacked PRs.
