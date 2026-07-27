# Rendering, physics and content standards

This document defines the production rules enforced by `ZoneUAContentStandards`.

## Rendering policy

Use sorting layers and `sortingOrder` for 2D draw order. Avoid arbitrary transform Z offsets on objects containing `SpriteRenderer`.

Required sorting layers:

1. `BackGroundTiles`
2. `GeneratedAndPlayer`
3. `TargetMouse`
4. `TopLayer`
5. `UI`

Prefab hierarchies containing multiple sprite renderers should normally have one `SortingGroup` at the visual root. This keeps body parts, weapons and effects together when actors overlap.

Negative transform scale is reported because it can invert collider geometry and complicate nested transforms. Prefer `SpriteRenderer.flipX` and `flipY` for visual facing where practical.

## Physics layers

The project defines these first-party physics layers:

- `Player`
- `NPC`
- `Projectile`
- `World`
- `Interactable`

Recommended collision intent:

| Layer | Expected interaction |
|---|---|
| Player | World, Interactable, hostile projectiles |
| NPC | World, Interactable, hostile projectiles |
| Projectile | World and damageable actors according to faction policy |
| World | Actors and projectiles |
| Interactable | Player/NPC sensors as required |

Review the Physics 2D collision matrix in Unity after migration. The repository does not guess faction-specific collision behaviour; `FactionMember` remains authoritative for damage permission.

## Prefab transforms

Production prefab roots should normally use:

```text
Position: 0, 0, 0
Rotation: 0, 0, 0
Scale:    1, 1, 1
```

Visual sizing belongs on child transforms or sprite import settings. Physics colliders should remain easy to inspect and should not rely on deeply compounded scales.

The validator reports zero-size `BoxCollider2D` and zero-radius `CircleCollider2D` as errors.

## Sprite import policy

The current pixel-art recommendation is:

```text
Pixels Per Unit: 16
Filter Mode: Point
Mip Maps: disabled
```

These are warnings rather than errors because UI art, large backgrounds or non-pixel assets may intentionally use different settings. Review every warning and document approved exceptions.

## Materials

Identical first-party materials are reported as likely duplicates. Reuse shared material assets to reduce state changes and avoid configuration drift.

The duplicate check is advisory. Two assets may intentionally remain separate when future independent tuning is expected.

## Editor workflow

Run:

```text
Zone UA → Validation → Validate Rendering and Physics
```

The validator checks:

- required sorting layers;
- production physics layers;
- prefab root transforms;
- negative hierarchy scales;
- arbitrary sprite Z offsets;
- missing `SortingGroup` on multi-renderer prefabs;
- invalid collider dimensions;
- sprite PPU, filtering and mipmaps;
- duplicate material content.

Content-standard errors also block Unity builds through `ZoneUAContentStandardsBuildValidator`. Warnings remain non-blocking.

## Migration order

1. Open the project in Unity 6000.5.5f1.
2. Confirm the new physics layer names in Project Settings.
3. Review and configure the Physics 2D collision matrix.
4. Run the content validator.
5. Fix errors first.
6. Review sprite-import warnings by asset category.
7. Replace arbitrary Z sorting with sorting layers and order.
8. Add `SortingGroup` to composed actor and weapon visuals where needed.
9. Re-run production composition and project validation.
10. Run representative overlap, projectile and collision smoke tests.

## Smoke-test checklist

- player and NPC sprites overlap in a stable order;
- equipped weapons remain grouped with their actor;
- grass and terrain do not pop in front of actors incorrectly;
- target indicators and top-layer effects remain visible;
- UI renders above world content;
- player and NPC collide with world geometry;
- projectiles hit only intended world and actor layers;
- faction rules still prevent disallowed damage;
- flipped actors retain correct collider behaviour;
- no production collider has zero dimensions;
- representative scenes show no unexpected material or texture changes.
