# Editor validation and CI

This module adds two complementary validation layers:

1. Fast repository checks that run without Unity.
2. Unity Editor validation that understands prefabs, ScriptableObjects and build preprocessing.

## Unity Editor menu

Open:

`Zone UA -> Validation -> Validate Project`

The validation window reports errors and warnings and can select affected assets.

For a smaller migration pass, select one or more prefabs and run:

`Zone UA -> Validation -> Validate Selected Prefabs`

## Checks performed in Unity

The project validator checks:

- missing `.meta` files;
- missing or duplicate GUIDs;
- invalid or duplicate asmdef names;
- required Player input map and actions;
- empty Build Settings scene list;
- missing world fallback biome or biome list;
- empty definition IDs;
- biomes without terrain presentation prefabs;
- NPC prefabs without `Health`, `Death` or `FactionMember`;
- weapon prefabs without `Weapon`;
- player prefabs with an unassigned `PlayerInputRouter.actions` field;
- scene metadata.

Errors block builds through `IPreprocessBuildWithReport`. Warnings remain visible but do not block a build.

A JSON report is written to:

`Logs/ZoneUAValidation.json`

## Batch mode

Run project validation from a Unity installation with:

```bash
Unity \
  -batchmode \
  -nographics \
  -quit \
  -projectPath . \
  -executeMethod ZoneUA.EditorValidation.ZoneUAValidationCli.Run \
  -logFile Logs/ZoneUAValidation.log
```

The process throws when validation errors exist.

## Repository validation

Run locally without Unity:

```bash
python scripts/validate_unity_repo.py
```

This verifies:

- `.meta` coverage;
- duplicate GUIDs;
- asmdef JSON and unique names;
- Input Actions JSON and required actions;
- Input System package declaration;
- project-version metadata.

The current repository still declares Unity `2022.2.8f1`. This is reported as a warning until the project is opened and saved in the target Unity `6000.5.5f1` editor. Do not manually invent the `m_EditorVersionWithRevision` value.

## GitHub Actions

Workflow:

`.github/workflows/unity-validation.yml`

The `repository-integrity` job runs on every pull request and on pushes to `master`. It does not need a Unity licence.

Unity EditMode tests are opt-in until the repository has working Unity CI credentials. Configure:

Repository variable:

- `RUN_UNITY_TESTS=true`
- `UNITY_VERSION=6000.5.5f1`

Repository secrets required by GameCI:

- `UNITY_LICENSE`
- `UNITY_EMAIL`
- `UNITY_PASSWORD`

After configuration, the workflow restores the Unity `Library` cache, runs all EditMode tests and uploads test artefacts.

## Recommended merge gate

Require the following check immediately:

- `Repository integrity`

Require the following after GameCI credentials and the migrated Unity project version are committed:

- `Unity EditMode tests`

## Current limitations

The validator intentionally does not modify scenes or prefabs automatically. It identifies unsafe setup while preserving serialized references. Visual world-generation quality, dual-grid transitions, animation bindings and Play Mode combat behaviour still require smoke testing in Unity.
