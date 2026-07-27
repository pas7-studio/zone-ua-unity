# Legacy cleanup and integration hardening

This module prevents new gameplay code from reintroducing integration paths that the professionalisation refactor has replaced.

## What is blocked

The Editor and repository scanners report blocking errors for:

- legacy `UnityEngine.Input` polling in runtime gameplay code;
- direct access from gameplay code to the ammo HUD through `GlobalSystem`.

The scanners report migration warnings for:

- Player/Enemy tag-based combat decisions;
- legacy lowercase Health API calls;
- scene-wide `FindObjectOfType`, `FindObjectsOfType` and `GameObject.Find` usage;
- other explicitly catalogued compatibility paths.

Warnings remain non-blocking because some existing prefab and UnityEvent migrations must be completed in the Unity Editor before those APIs can be removed safely.

## Unity Editor workflow

Run:

`Zone UA -> Validation -> Scan Legacy Integration`

The scan writes:

`Logs/ZoneUALegacyUsage.json`

Each finding includes:

- rule code;
- severity;
- source path;
- line number;
- source line;
- required migration direction.

The same blocking scan runs before a Unity build.

## Repository workflow

Run without Unity:

```bash
python scripts/check_legacy_usage.py
```

The script writes:

`Logs/legacy-usage.json`

It returns a non-zero exit code when blocking findings are present and is executed by the repository-integrity GitHub Actions job.

## Deprecated compatibility APIs

The following methods remain temporarily available for serialized UnityEvents but now produce compiler warnings when called from C#:

- `Health.ReceiveDamage(int)`;
- `Health.HealthLogic()`;
- `Health.setHeals(int)`;
- `Health.restoreSomeHeals(int)`;
- `Health.restoreDefaultHeals()`;
- `Health.getHeals()`;
- `Health.receiveDamage(int)`;
- `Health.getIsAlive()`;
- `GlobalSystem.getRandomBlood()`;
- `GlobalSystem.AmmoUI`.

New code must use the modern event-driven and typed APIs.

## Removal process

Do not delete compatibility methods until all of the following are true:

1. production prefabs have been migrated with the composition migration tooling;
2. production scenes have been migrated and saved in Unity;
3. UnityEvent persistent calls have been reviewed for missing methods;
4. the legacy scan contains no usages of the API outside the compatibility declaration itself;
5. EditMode and Play Mode smoke tests pass;
6. a full project validation and build complete without errors.

After those checks, remove one compatibility group per PR. Do not remove Health, weapon, input and scene compatibility bridges in one bulk change.

## Final stacked-series verification

Before merging the stacked refactor series:

1. open the project in Unity 6000.5.5f1;
2. generate and review the scene architecture;
3. run production composition migration;
4. assign definitions and serialized references;
5. run all validation menus;
6. run `python scripts/validate_unity_repo.py`;
7. run `python scripts/check_legacy_usage.py`;
8. run the complete EditMode suite;
9. run player, NPC, combat, world, pooling, scene transition and HUD Play Mode smoke tests;
10. record a representative performance capture;
11. merge stacked PRs in dependency order or squash the verified final branch into a clean integration PR.

This module is a regression gate. It does not claim that every old serialized UnityEvent has already been migrated; that verification requires the Unity Editor.
