# World generation definitions migration

Target editor: Unity 6 / 6000.5.5f1.

This module moves world-generation configuration out of `MapGenerator` and into reusable assets while preserving the old arrays and height-threshold path as a temporary prefab fallback.

## Runtime structure

- `WorldGenerationSettings` owns seed, grid size, tile size, chunk tuning, noise scales, decoration spacing and biome references.
- `BiomeDefinition` owns climate ranges, terrain prefab and deterministic decoration options.
- `WorldGenerationContext` derives independent noise offsets from one seed.
- `WorldNoiseSampler` samples elevation, moisture, temperature, vegetation and settlement channels.
- `WorldDeterminism` supplies stable per-cell values for decoration chance, prefab selection, jitter and rotation.
- `MapGenerator` is the scene adapter that instantiates terrain and invokes existing tile content such as `GrassGenerator`.

The module does not replace the project's dual-grid rendering or transition presentation. It supplies deterministic biome data and editor-visible configuration that those presentation systems consume.

## Asset setup

Create biome assets through:

`Assets > Create > Zone UA > World > Biome Definition`

For each biome configure:

1. unique `id`;
2. display name;
3. elevation range;
4. moisture range;
5. temperature range;
6. terrain or dual-grid presentation prefab;
7. optional decoration prefabs;
8. decoration density.

Create one settings asset through:

`Assets > Create > Zone UA > World > Generation Settings`

Configure:

- fixed or runtime seed;
- map width and height;
- tile size;
- chunk values;
- all five noise scales;
- minimum decoration distance;
- decoration density multiplier;
- ordered biome definitions;
- fallback biome.

Biome order is intentional: the first matching climate range wins. Put narrow/specialised ranges before broad ranges.

## Scene migration

1. Assign the `WorldGenerationSettings` asset to `MapGenerator.settings`.
2. Optionally assign a dedicated `generationRoot`.
3. Point `ChunkManager.chunkRoot` to the same generated root when a separate root is used.
4. Use `Validate Generation Settings` from the `MapGenerator` component context menu.
5. Regenerate with a fixed seed and record a reference screenshot.
6. Regenerate again with the same seed and verify identical terrain and decoration placement.
7. Only after prefab and scene validation remove reliance on legacy arrays.

## Validation rules

The settings validator reports:

- missing fallback biome;
- empty biome array;
- empty biome slots;
- duplicated biome ids;
- biome definitions without terrain prefabs.

Climate overlap is allowed because ordered matching supports specialised biomes. Designers must review the order explicitly.

## Determinism contract

The same settings, seed and grid coordinate produce the same:

- noise sample;
- resolved biome;
- decoration chance;
- decoration prefab index;
- decoration jitter;
- decoration rotation.

Changing biome array order, climate ranges, noise scales, map coordinate origin or prefab arrays changes generated output and should be treated as a world-generation version change.

## Play Mode checklist

- No Console compilation errors.
- Fixed seed regenerates identical terrain.
- Runtime seed changes `LastResolvedSeed` and generated output.
- Every generated cell resolves a biome or the fallback biome.
- Existing dual-grid transitions remain visually smooth.
- Decoration spacing respects `MinimumDecorationDistance`.
- Empty decoration entries do not abort map generation.
- `GrassGenerator` still runs once for generated terrain prefabs that contain it.
- `ChunkManager` tracks the selected generation root.
- Legacy scenes without a settings asset still use old tile arrays.
- Regeneration cleans previous terrain and decoration instances.

## Follow-up

After all scenes are migrated and saved by Unity, remove the legacy `MapGenerator` fields in a dedicated cleanup PR. Do not remove them in the same commit that first assigns settings assets because Unity serialization references must be verified locally.
