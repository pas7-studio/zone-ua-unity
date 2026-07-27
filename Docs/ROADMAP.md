# Zone UA professionalisation roadmap

Target editor: Unity 6 (6000.5.5f1).

This roadmap turns the approved technical backlog into independently verifiable phases. Each phase should compile, preserve serialized references, and pass a focused Play Mode smoke test before the next phase begins.

## Phase 0 — Unity 6 baseline

- Bring the already-migrated `ProjectSettings`, `Packages`, scenes and prefabs into the refactor branch.
- Resolve compile errors, obsolete API warnings, missing scripts and broken serialized references.
- Verify render pipeline, Input System, TextMeshPro and package compatibility.

Exit criteria:

- zero compile errors;
- no missing scripts in production scenes/prefabs;
- movement, camera, combat, NPC, world generation and UI smoke tests pass.

## Phase 1 — Project boundaries

- Move first-party assets under `Assets/_ZoneUA` and external content under `Assets/ThirdParty` through the Unity Editor while retaining `.meta` files.
- Split runtime code into Core, Characters, Combat, AI, World, Camera, UI and Utilities.
- Add `ZoneUA.*` namespaces.
- Add assembly definitions with explicit one-way dependencies.
- Keep `GlobalSystem` as a scene composition root only.
- Replace global searches with serialized composition, interfaces, events or explicit registration.

Exit criteria:

- assemblies compile independently;
- no gameplay class depends on concrete UI implementations;
- no new `Find*` calls in runtime hot paths;
- scene dependencies are visible in Inspector.

## Phase 2 — Definitions and Inspector contracts

Create ScriptableObject definitions:

- `WeaponDefinition`;
- `ProjectileDefinition`;
- `NpcDefinition`;
- `FactionDefinition`;
- `WorldGenerationSettings`;
- `BiomeDefinition`;
- `DamageEffectSettings`.

Separate immutable definition data, runtime state and scene references. Add `Min`, `Range`, `Tooltip`, logical headers, `FormerlySerializedAs` and `OnValidate` where appropriate. Hide runtime state from the normal Inspector.

Exit criteria:

- duplicated balance values are removed from weapon/NPC prefabs;
- invalid definitions report clear validation errors;
- prefabs reference definitions instead of duplicating configuration.

## Phase 3 — Combat architecture

Split the current weapon implementation into focused responsibilities:

- `WeaponController` orchestration;
- `WeaponInput`;
- `WeaponAim`;
- `WeaponFire`;
- `WeaponReload`;
- `WeaponRecoil`;
- `ProjectileSpawner`;
- `ShellEjector`;
- `WeaponAudio`.

Expose the command API:

- `StartFire()`;
- `StopFire()`;
- `Reload()`;
- `SetAimTarget()`;
- `SwitchFireMode()`.

Use events for ammo, reload and fire-mode changes. Validate supported fire modes. Separate weapon recoil, camera recoil and projectile spread. Replace per-shot coroutine allocation with state/timer-based firing.

Exit criteria:

- player and NPC use the same weapon command API;
- weapon code has no direct HUD dependency;
- burst interruption, reload interruption and weapon switching are covered by tests.

## Phase 4 — Damage, health and death

- Introduce `DamageInfo` and `DamageType`.
- Move damage and critical calculation out of `Bullet`.
- Add `Damaged`, `HealthChanged`, `Healed` and `Died` events.
- Move blood/effect presentation out of `Health`.
- Make death a complete state that disables input, AI and weapon control and applies collider, animation, loot and despawn policy.
- Remove legacy health methods only after checking scenes, prefabs and UnityEvents.

Exit criteria:

- all damage sources use `DamageInfo`;
- death is idempotent;
- health domain code does not instantiate visual effects.

## Phase 5 — Factions and AI

- Add `FactionMember`, faction relationships and friendly-fire policy.
- Remove combat decisions based on `Player`/`Enemy` tags.
- Split NPC logic into state machine, `TargetSensor`, `NpcMovement`, `NpcCombat` and `PatrolBehaviour`.
- Support Idle, Patrol, Investigate, Chase, Attack, Reload, Search, Flee and Dead states.
- Add layer-mask sensing, line of sight, last-known position, target scoring and faction filtering.
- Add reaction to weapon noise.
- Distribute AI decision ticks across frames with a configurable budget.

Exit criteria:

- NPC decisions are state-driven;
- physics movement remains in fixed-step execution;
- hostile selection is faction- and visibility-aware;
- AI scanning does not spike all NPCs in one frame.

## Phase 6 — Input System

- Create actions for Move, Aim, Fire, Reload, NextWeapon, PreviousWeapon, Interact, Sprint and Pause.
- Add `PlayerInputReader`.
- Remove legacy input calls from gameplay components.
- Support keyboard/mouse and gamepad, with a path for mobile controls and remapping.

Exit criteria:

- gameplay receives commands/state from an input abstraction;
- no weapon or movement component calls legacy `Input.Get*` APIs.

## Phase 7 — Runtime pooling

- Finish projectile, shell, damage-popup, blood and particle pooling.
- Add per-prefab prewarm and maximum capacity.
- Add double-release protection and `IPoolable` lifecycle callbacks.
- Reset rigidbodies, trails, particles, animators, timers, hit flags and target references.
- Clear scene-local pools on scene teardown.
- Expose statistics only in Editor or Development builds.

Exit criteria:

- normal sustained combat produces no repeated projectile/effect instantiate-destroy churn;
- pools remain bounded;
- pooled objects pass state-reset tests.

## Phase 8 — World generation

Split generation into data, biome selection, tile placement, decoration placement and chunk view. Keep seed and local random state deterministic. Add chunk streaming, staged generation, distant unload, pooled views, biome blending, layered noise, deterministic placement and minimum-distance rules. Keep logical world data separate from rendering.

Exit criteria:

- the same seed produces the same world data;
- generation does not mutate Unity global random state;
- chunk generation/unload is frame-budgeted;
- generated-object counts remain bounded around the player.

## Phase 9 — Rendering, physics, UI and content validation

- Standardise sorting layers, order rules, physics layers and collision matrix.
- Replace arbitrary Z offsets with sorting policy and `SortingGroup`.
- Audit grass rendering, sprite atlases, Pixels Per Unit, compression, mipmaps, filtering, overdraw and duplicate materials.
- Decouple HUD from gameplay through events.
- Normalise prefab roots, pivots, colliders and naming.
- Split scenes into Production, Development and Tests and add a Bootstrap scene.

Exit criteria:

- project validators report no critical prefab/scene violations;
- UI updates do not rebuild unrelated elements;
- rendering and physics rules are documented and consistently applied.

## Phase 10 — Tests, tooling, CI and budgets

- Add EditMode and PlayMode coverage for damage, factions, fire modes, map determinism, pooling, movement, reload, death, NPC sensing and regeneration.
- Add `Zone UA/Validate Project` tooling for missing scripts/references, layers, tags, duplicate materials, prefab scale, scene references and missing definitions.
- Add selected gizmos and custom inspectors.
- Add Unity-compatible CI for compile, tests and build validation.
- Add Git LFS when large binary growth justifies it.
- Create a representative stress scene and define CPU, GPU, GC, batching, physics and active-object budgets.

Exit criteria:

- pull requests receive automated compile/test feedback;
- the stress scene has recorded baseline numbers;
- performance work is driven by measured budgets rather than guesses.

## Delivery rule

Do not combine architecture migration, gameplay redesign, asset relocation and performance tuning into one unreviewable change. Every phase must preserve `.meta` GUIDs, state its migration requirements and include a focused validation checklist.
