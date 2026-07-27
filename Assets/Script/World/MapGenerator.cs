using System.Collections.Generic;
using UnityEngine;

public sealed class MapGenerator : MonoBehaviour
{
    [Header("Map")]
    [SerializeField, Min(1)] private int mapWidth = 1;
    [SerializeField, Min(1)] private int mapHeight = 1;
    [SerializeField, Min(0.01f)] private float tileSize = 1f;
    [SerializeField, Min(0.0001f)] private float heightScale = 1f;

    [Header("Seed")]
    [SerializeField] private int seed;
    [SerializeField] private bool useRandomSeed;

    [Header("Biomes")]
    [SerializeField] private GameObject[] tilePrefabs;
    [SerializeField] private float[] heightThresholds;
    [SerializeField] private GameObject[] specialPrefabs;

    [Header("Runtime")]
    [SerializeField] private bool enableChunkManager = true;
    [SerializeField] private bool testGenerationMode;
    [SerializeField, Min(0.1f)] private float generationInterval = 5f;

    private readonly List<GameObject> spawnedTiles = new List<GameObject>();

    private ChunkManager chunkManager;
    private float timeSinceLastGeneration;

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
        ClearTiles();

        if (useRandomSeed)
        {
            seed = UnityEngine.Random.Range(0, 100000);
        }

        GenerateBiomesAndGrass();

        if (chunkManager != null)
        {
            chunkManager.RefreshChunks();
            chunkManager.enabled = enableChunkManager;
        }
    }

    private void GenerateBiomesAndGrass()
    {
        if (tilePrefabs == null || tilePrefabs.Length == 0)
        {
            Debug.LogWarning("Map generation skipped: no tile prefabs configured.", this);
            return;
        }

        System.Random random = new System.Random(seed);
        float xOffset = Mathf.Lerp(-10000f, 10000f, (float)random.NextDouble());
        float yOffset = Mathf.Lerp(-10000f, 10000f, (float)random.NextDouble());

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float xCoordinate = (float)x / mapWidth * heightScale + xOffset;
                float yCoordinate = (float)y / mapHeight * heightScale + yOffset;
                float heightValue = Mathf.PerlinNoise(xCoordinate, yCoordinate);

                GameObject tilePrefab = ChooseTilePrefab(heightValue);
                if (tilePrefab == null)
                {
                    continue;
                }

                GameObject tile = Instantiate(tilePrefab, transform);
                tile.transform.localPosition = new Vector3(x * tileSize, y * tileSize, 0f);
                spawnedTiles.Add(tile);

                if (tile.TryGetComponent(out GrassGenerator grassGenerator))
                {
                    grassGenerator.GenerateGrass();
                }
            }
        }
    }

    private void ClearTiles()
    {
        for (int i = 0; i < spawnedTiles.Count; i++)
        {
            if (spawnedTiles[i] != null)
            {
                Destroy(spawnedTiles[i]);
            }
        }

        spawnedTiles.Clear();
    }

    private GameObject ChooseTilePrefab(float heightValue)
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
