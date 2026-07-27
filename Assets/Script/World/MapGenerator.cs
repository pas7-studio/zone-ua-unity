using System;
using System.Collections.Generic;
using UnityEngine;
using ZoneUA.World;

[DisallowMultipleComponent]
public sealed class MapGenerator : MonoBehaviour
{
    [Header("Definition-driven Generation")]
    [SerializeField, Tooltip("Authoritative world generation configuration. Legacy fields remain as a migration fallback.")]
    private WorldGenerationSettings settings;
    [SerializeField, Tooltip("Optional parent for generated terrain and decorations.")]
    private Transform generationRoot;

    [Header("Legacy Map Fallback")]
    [SerializeField, Min(1)] private int mapWidth = 1;
    [SerializeField, Min(1)] private int mapHeight = 1;
    [SerializeField, Min(0.01f)] private float tileSize = 1f;
    [SerializeField, Min(0.0001f)] private float heightScale = 1f;

    [Header("Legacy Seed Fallback")]
    [SerializeField] private int seed;
    [SerializeField] private bool useRandomSeed;

    [Header("Legacy Biome Fallback")]
    [SerializeField] private GameObject[] tilePrefabs;
    [SerializeField] private float[] heightThresholds;
    [SerializeField] private GameObject[] specialPrefabs;

    [Header("Runtime")]
    [SerializeField] private bool enableChunkManager = true;
    [SerializeField] private bool testGenerationMode;
    [SerializeField, Min(0.1f)] private float generationInterval = 5f;
    [SerializeField, HideInInspector] private int lastResolvedSeed;

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();
    private readonly List<Vector2> decorationPositions = new List<Vector2>();

    private ChunkManager chunkManager;
    private float timeSinceLastGeneration;

    public event Action<int> GenerationCompleted;

    public WorldGenerationSettings Settings => settings;
    public int LastResolvedSeed => lastResolvedSeed;

    private Transform OutputRoot => generationRoot != null ? generationRoot : transform;

    private void Awake()
    {
        chunkManager = GetComponent<ChunkManager>();
    }

    private void Start()
    {
        Regenerate();
    }

    private void Update()
    {
        if (!testGenerationMode)
        {
            return;
        }

        timeSinceLastGeneration += Time.deltaTime;
        if (timeSinceLastGeneration < generationInterval)
        {
            return;
        }

        timeSinceLastGeneration = 0f;
        Regenerate();
    }

    [ContextMenu("Regenerate Map")]
    public void Regenerate()
    {
        ClearGeneratedObjects();

        if (settings != null)
        {
            GenerateFromSettings();
        }
        else
        {
            GenerateLegacy();
        }

        if (chunkManager != null)
        {
            chunkManager.RefreshChunks();
            chunkManager.enabled = enableChunkManager;
        }

        GenerationCompleted?.Invoke(lastResolvedSeed);
    }

    [ContextMenu("Validate Generation Settings")]
    private void ValidateGenerationSettings()
    {
        if (settings == null)
        {
            Debug.LogWarning("MapGenerator uses legacy fallback fields because no WorldGenerationSettings asset is assigned.", this);
            return;
        }

        var errors = new List<string>();
        settings.CollectValidationErrors(errors);
        if (errors.Count == 0)
        {
            Debug.Log($"World generation settings '{settings.name}' are valid.", settings);
            return;
        }

        for (int i = 0; i < errors.Count; i++)
        {
            Debug.LogError(errors[i], settings);
        }
    }

    private void GenerateFromSettings()
    {
        int runtimeSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        lastResolvedSeed = settings.ResolveSeed(runtimeSeed);
        var context = new WorldGenerationContext(lastResolvedSeed);
        decorationPositions.Clear();

        for (int x = 0; x < settings.MapWidth; x++)
        {
            for (int y = 0; y < settings.MapHeight; y++)
            {
                WorldSample sample = WorldNoiseSampler.Sample(settings, in context, x, y);
                BiomeDefinition biome = settings.ResolveBiome(
                    sample.Elevation,
                    sample.Moisture,
                    sample.Temperature);

                if (biome == null || biome.TerrainPrefab == null)
                {
                    continue;
                }

                Vector3 localPosition = new Vector3(
                    x * settings.TileSize,
                    y * settings.TileSize,
                    0f);

                GameObject tile = Instantiate(biome.TerrainPrefab, OutputRoot);
                tile.transform.localPosition = localPosition;
                spawnedObjects.Add(tile);
                GenerateTileContent(tile);
                TryGenerateDecoration(biome, sample, x, y, localPosition);
            }
        }
    }

    private void TryGenerateDecoration(
        BiomeDefinition biome,
        in WorldSample sample,
        int x,
        int y,
        Vector3 tileLocalPosition)
    {
        GameObject[] prefabs = biome.DecorationPrefabs;
        if (prefabs == null || prefabs.Length == 0)
        {
            return;
        }

        float chance = Mathf.Clamp01(
            biome.DecorationDensity *
            settings.DecorationDensityMultiplier *
            sample.Vegetation);

        if (WorldDeterminism.Value01(lastResolvedSeed, x, y, 101) > chance)
        {
            return;
        }

        int prefabIndex = WorldDeterminism.Index(lastResolvedSeed, x, y, prefabs.Length, 211);
        GameObject prefab = prefabIndex >= 0 ? prefabs[prefabIndex] : null;
        if (prefab == null)
        {
            return;
        }

        float jitterRange = settings.TileSize * 0.35f;
        Vector2 jitter = new Vector2(
            Mathf.Lerp(-jitterRange, jitterRange, WorldDeterminism.Value01(lastResolvedSeed, x, y, 307)),
            Mathf.Lerp(-jitterRange, jitterRange, WorldDeterminism.Value01(lastResolvedSeed, x, y, 401)));
        Vector2 candidateLocalPosition = (Vector2)tileLocalPosition + jitter;

        float minimumDistance = settings.MinimumDecorationDistance;
        float minimumDistanceSquared = minimumDistance * minimumDistance;
        for (int i = 0; i < decorationPositions.Count; i++)
        {
            if ((decorationPositions[i] - candidateLocalPosition).sqrMagnitude < minimumDistanceSquared)
            {
                return;
            }
        }

        float angle = WorldDeterminism.Value01(lastResolvedSeed, x, y, 503) * 360f;
        GameObject decoration = Instantiate(
            prefab,
            OutputRoot.TransformPoint(candidateLocalPosition),
            Quaternion.Euler(0f, 0f, angle),
            OutputRoot);
        spawnedObjects.Add(decoration);
        decorationPositions.Add(candidateLocalPosition);
    }

    private void GenerateLegacy()
    {
        if (tilePrefabs == null || tilePrefabs.Length == 0)
        {
            Debug.LogWarning("Map generation skipped: no WorldGenerationSettings or legacy tile prefabs configured.", this);
            return;
        }

        if (useRandomSeed)
        {
            seed = UnityEngine.Random.Range(0, 100000);
        }

        lastResolvedSeed = seed;
        var random = new System.Random(seed);
        float xOffset = Mathf.Lerp(-10000f, 10000f, (float)random.NextDouble());
        float yOffset = Mathf.Lerp(-10000f, 10000f, (float)random.NextDouble());

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float xCoordinate = (float)x / mapWidth * heightScale + xOffset;
                float yCoordinate = (float)y / mapHeight * heightScale + yOffset;
                float heightValue = Mathf.PerlinNoise(xCoordinate, yCoordinate);
                GameObject tilePrefab = ChooseLegacyTilePrefab(heightValue);
                if (tilePrefab == null)
                {
                    continue;
                }

                GameObject tile = Instantiate(tilePrefab, OutputRoot);
                tile.transform.localPosition = new Vector3(x * tileSize, y * tileSize, 0f);
                spawnedObjects.Add(tile);
                GenerateTileContent(tile);
            }
        }
    }

    private static void GenerateTileContent(GameObject tile)
    {
        if (tile != null && tile.TryGetComponent(out GrassGenerator grassGenerator))
        {
            grassGenerator.GenerateGrass();
        }
    }

    private void ClearGeneratedObjects()
    {
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            GameObject instance = spawnedObjects[i];
            if (instance == null)
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(instance);
            }
            else
            {
                DestroyImmediate(instance);
            }
        }

        spawnedObjects.Clear();
        decorationPositions.Clear();
    }

    private GameObject ChooseLegacyTilePrefab(float heightValue)
    {
        int thresholdCount = heightThresholds != null ? heightThresholds.Length : 0;
        int specialCount = specialPrefabs != null ? specialPrefabs.Length : 0;
        int count = Mathf.Min(thresholdCount, specialCount);

        for (int i = 0; i < count; i++)
        {
            if (heightValue <= heightThresholds[i] && specialPrefabs[i] != null)
            {
                return specialPrefabs[i];
            }
        }

        return tilePrefabs[tilePrefabs.Length - 1];
    }

    private void OnValidate()
    {
        mapWidth = Mathf.Max(1, mapWidth);
        mapHeight = Mathf.Max(1, mapHeight);
        tileSize = Mathf.Max(0.01f, tileSize);
        heightScale = Mathf.Max(0.0001f, heightScale);
        generationInterval = Mathf.Max(0.1f, generationInterval);
    }
}
