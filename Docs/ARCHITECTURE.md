# Zone UA Unity architecture

## Goals

1. Preserve playable behaviour and serialized Unity references while modernising the codebase.
2. Keep configuration in the Inspector and runtime state encapsulated.
3. Remove repeated scene searches, component lookups and avoidable allocations.
4. Separate gameplay domains so future features can be added without growing god objects.

## Runtime ownership

### `GlobalSystem`

`GlobalSystem` is the scene composition root for shared references and global visual settings. It registers a scene-local `Instance` during `Awake`; other systems no longer perform repeated tag searches.

It should only contain references or configuration that is genuinely shared. Game rules should move into focused services or ScriptableObject configurations in later changes.

### Characters

`CharacterCustomController` reads input in `Update`, applies Rigidbody movement in `FixedUpdate`, and caches the main camera and Animator parameter hashes.

`Health` owns health state and invokes `Death` exactly once. Visual blood settings come from `GlobalSystem`.

### Combat

`Weapon` is weapon metadata. `WeaponController` owns firing, aiming, recoil and reload state. `WeaponSwitcher` only coordinates active weapon selection and ammo UI.

NPCs use the same public weapon API as the player rather than mutating implementation fields.

### NPC

`NPCController` has three update frequencies:

- short-term movement/aiming in `FixedUpdate`;
- medium-term target scanning at a configurable interval;
- long-term patrol decisions at a configurable interval.

Physics overlap queries use a reusable non-allocating buffer. Timers are reset after execution.

### World

`MapGenerator` generates deterministically without changing Unity's global random state. It tracks only objects it owns and clears that list when regenerating.

`ChunkManager` caches chunk renderers and reuses a frustum plane array. `CharacterChunks` enables only the current chunk sorter instead of scanning the entire scene.

## Target folder layout

Asset moves should be done as a dedicated Unity Editor migration so `.meta` GUIDs are retained and scene/prefab references can be validated:

```text
Assets/
  _ZoneUA/
    Art/
    Audio/
    Materials/
    Prefabs/
      Characters/
      Weapons/
      World/
      UI/
    Runtime/
      Core/
      Characters/
      Combat/
      AI/
      Camera/
      World/
      UI/
      Utilities/
    Scenes/
      Production/
      Development/
    Settings/
    Tests/
      EditMode/
      PlayMode/
  ThirdParty/
```

The baseline refactor does not mass-move assets through Git because a Unity Editor reimport is required to verify every serialized reference.

## Next technical steps

- Introduce ScriptableObject definitions for weapon, NPC and biome configuration.
- Replace bullets, shell casings, blood decals and damage popups with object pools.
- Split input from character movement, ideally using Unity Input System actions.
- Add EditMode tests for deterministic map selection and PlayMode tests for health, reload and NPC scan cadence.
- Replace tag-based hostility with factions/teams and layer masks.
- Profile generated scenes with Unity Profiler before changing rendering or physics budgets.
