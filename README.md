# Zone UA

A 2D Unity prototype with procedural world generation, NPC combat, weapons, vegetation sorting and supporting UI.

## Editor

- Unity **6000.5.5f1**
- Universal Render Pipeline 17
- Open `Assets/Scenes/SampleScene.unity` as the current main scene.

## Repository rules

- Never commit `Library`, `Temp`, `Obj`, `Logs`, `.vs`, `.idea` or `.plastic`.
- Keep every Unity asset together with its `.meta` file.
- Move or rename assets from the Unity Editor whenever possible.
- Runtime tuning belongs in private `[SerializeField]` fields, not hard-coded values.
- Runtime state is exposed through read-only properties and behaviour methods.
- Avoid `Find*`, `Camera.main`, LINQ and `GetComponent` inside per-frame or physics loops.

## Current architecture

The first refactor deliberately keeps existing asset GUIDs and most paths stable. Runtime code is grouped by responsibility under `Assets/Script`:

- `NPC` — NPC decision making and combat coordination.
- `Weapon` — projectile behaviour.
- `World` — generation, chunks and vegetation rendering.
- root scripts — character, camera, UI and shared gameplay components.

See [`Docs/ARCHITECTURE.md`](Docs/ARCHITECTURE.md) for the target folder layout and migration rules.
