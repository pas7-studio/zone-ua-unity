# Zone UA architecture

## Direction

The project is being migrated incrementally from a prototype-style collection of MonoBehaviours to a small, explicit runtime architecture. Unity asset GUIDs and serialized Inspector data must remain stable during that process.

## Runtime composition

`GlobalSystem` is the current scene composition root. It owns scene-level references and creates the runtime infrastructure used by gameplay systems.

Current responsibilities:

- exposes the ammo UI reference;
- owns the runtime object container;
- provides centralised spawn/release operations;
- owns `RuntimeObjectPool`;
- exposes temporary blood-effect configuration until those values are migrated into ScriptableObject definitions.

Gameplay components should not call `FindGameObjectWithTag("System")`. They should resolve `GlobalSystem.Instance` once and cache it.

## Runtime object lifetime

Frequently spawned transient objects must use `GlobalSystem.Spawn` and `GlobalSystem.Release` where practical.

The pool currently supports:

- blood decals;
- particle effects;
- any prefab spawned through the composition root;
- pool-aware `AutoDispose` objects.

The pool resets 2D/3D rigidbody velocities, trail renderers and particle systems before reuse. Objects not created by the pool safely fall back to normal `Destroy` behaviour.

Future work should integrate projectile and shell spawning directly into this API after Play Mode verification.

## Target asset layout

Asset moves must be performed inside the Unity Editor so `.meta` files move with their assets.

```text
Assets/
  _ZoneUA/
    Art/
      Animations/
      Materials/
      Shaders/
      Sprites/
    Audio/
      Music/
      SFX/
    Prefabs/
      Characters/
      Effects/
      UI/
      Weapons/
      World/
    Runtime/
      Camera/
      Characters/
      Combat/
      Core/
      UI/
      Weapons/
      World/
    Scenes/
      Development/
      Production/
    Settings/
    Tests/
      EditMode/
      PlayMode/
  ThirdParty/
```

## Script rules

- One public MonoBehaviour or ScriptableObject per file.
- Filename must match the public Unity type.
- Inspector configuration uses private `[SerializeField]` fields.
- Runtime state is exposed through read-only properties or explicit methods.
- Cache components in `Awake` or `Start`; avoid repeated `GetComponent` in hot loops.
- Use `Animator.StringToHash` for repeatedly accessed Animator parameters.
- Use squared distances for repeated range comparisons.
- Avoid LINQ, allocations and scene-wide searches in `Update`/`FixedUpdate`.
- Use `OnValidate` for local numeric constraints and reference diagnostics.
- Preserve renamed serialized fields with `FormerlySerializedAs`.
- Keep backwards-compatible UnityEvent methods until scenes and prefabs are migrated.

## Configuration migration

Large groups of balancing values should move from scene objects into ScriptableObject definitions. Do this one subsystem at a time and retain the existing serialized values as migration fallbacks until every prefab has been updated.

Recommended order:

1. weapon definitions and fire/recoil profiles;
2. character movement and health definitions;
3. NPC perception and patrol profiles;
4. biome and map-generation profiles;
5. visual-effects budgets and pooling prewarm settings.

## Assembly boundaries

Do not introduce assembly definition files until the asset folders have been moved in the Unity Editor and all current scripts compile. Afterwards, use small assembly boundaries such as:

- `ZoneUA.Core`;
- `ZoneUA.Gameplay`;
- `ZoneUA.World`;
- `ZoneUA.UI`;
- `ZoneUA.Editor`;
- `ZoneUA.Tests`.

Introducing asmdefs too early can expose hidden circular dependencies and block the entire project from compiling.

## Validation before merge

1. Open with Unity 2022.2.8f1.
2. Allow a clean import and inspect the Console.
3. Confirm that renamed scripts have no `Missing Script` components.
4. Smoke-test player movement, camera, weapon switching, reload, NPC patrol/combat, damage/death, map regeneration and grass sorting.
5. Verify pooled blood and particle effects across repeated hits.
6. Profile representative combat and generated-world scenes before selecting prewarm counts or hard pool limits.
7. Move assets only through the Unity Project window.
