# Bootstrap and scene architecture

This module separates persistent services from gameplay content and gives the project a deterministic additive-loading path.

## Scene roles

- `Bootstrap`: persistent composition root and the first enabled Build Settings scene.
- `Production`: normal gameplay content.
- `Development`: developer-only experiments and diagnostics.
- `Tests`: Play Mode and smoke-test content.

The development and test scenes are created disabled in Build Settings.

## Scene catalog

Create or update the standard layout with:

`Zone UA -> Scenes -> Create Scene Architecture`

The command creates:

- `Assets/_ZoneUA/Settings/SceneCatalog.asset`;
- `Assets/_ZoneUA/Scenes/Bootstrap/Bootstrap.unity`;
- `Assets/_ZoneUA/Scenes/Production/Production.unity`;
- `Assets/_ZoneUA/Scenes/Development/Development.unity`;
- `Assets/_ZoneUA/Scenes/Tests/Tests.unity`.

Existing files are not overwritten. Open scenes are preserved while missing templates are generated additively.

## Bootstrap composition

The generated bootstrap root contains, when the runtime scripts are compiled:

- `GlobalSystem`;
- `SceneBootstrapper`;
- a reference to `SceneCatalog`.

Keep persistent services only in Bootstrap. Do not duplicate them in Production, Development or Tests.

Recommended persistent services:

- runtime object pool composition root;
- scene transition service;
- save/settings services;
- audio root;
- analytics or diagnostics roots that explicitly survive scene changes.

Gameplay actors, world generation, cameras and scene-local UI should remain in gameplay scenes unless they have a deliberate persistent lifecycle.

## Runtime transitions

`SceneBootstrapper` loads gameplay scenes additively, activates the new scene and then unloads the previous gameplay scene.

Public entry points:

- `LoadInitialScene()`;
- `LoadScene(string sceneName)`;
- `CancelTransition()`.

Runtime state is exposed through `SceneTransitionState`:

- `Idle`;
- `Loading`;
- `Activating`;
- `Unloading`;
- `Completed`;
- `Failed`.

The state object rejects overlapping transitions and clamps progress to `0..1`.

## Validation

Run:

`Zone UA -> Scenes -> Validate Scene Architecture`

The validator checks:

- `SceneCatalog` exists;
- every configured scene name resolves to a scene asset;
- Bootstrap is the first enabled Build Settings scene;
- Bootstrap and Production templates exist.

The same checks run before a Unity build and block the build on errors.

## Migration order

1. Open the project in Unity 6000.5.5f1.
2. Run `Create Scene Architecture`.
3. Open the generated Bootstrap scene and inspect its components.
4. Move persistent services from the old gameplay scene into Bootstrap through the Unity Editor.
5. Move gameplay-only objects into Production.
6. Keep stress harnesses and debug tools in Development.
7. Keep automated Play Mode fixtures in Tests.
8. Remove duplicated persistent services from non-bootstrap scenes.
9. Run scene validation.
10. Run movement, combat, NPC, world, HUD and scene-transition smoke tests.

## Required smoke tests

- launching the build starts from Bootstrap;
- Production loads additively and becomes active;
- Bootstrap remains loaded after transitions;
- the previous gameplay scene unloads;
- only one `SceneBootstrapper` and one `GlobalSystem` exist;
- pooled objects do not survive into the wrong gameplay scene unless explicitly persistent;
- player input is disabled or routed safely during transition;
- loading failure reports an error and does not start another overlapping transition;
- Development and Tests remain disabled in release Build Settings.

## Important limitation

The repository change provides runtime code and Editor tooling. The generated `.unity` assets must be created and reviewed locally in Unity so Unity owns their serialization, GUIDs and object references.
