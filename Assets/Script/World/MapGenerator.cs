using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int mapWidth;
    public int mapHeight;
    public float tileSize;
    public float heightScale;

    public int seed;
    public bool useRandomSeed;

    public GameObject[] tilePrefabs;
    public float[] heightThresholds;
    public GameObject[] specialPrefabs;

    public bool enableChunkManager = true;
    private ChunkManager chunkManager;

    public bool testGenerationMode = false;
    public float generationInterval = 5f;
    private float timeSinceLastGeneration = 0f;

    private TestWorldSorting worldSorting;
    private List<GameObject> spawnedTiles = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        if (useRandomSeed)
        {
            seed = Random.Range(0, 100000);
        }

        worldSorting = GetComponent<TestWorldSorting>();
        chunkManager = GetComponent<ChunkManager>();
        Generation();
    }

    // Update is called once per frame
    void Update()
    {
        if (testGenerationMode)
        {
            timeSinceLastGeneration += Time.deltaTime;
            if (timeSinceLastGeneration >= generationInterval)
            {
                ClearTiles();
                Generation();
                timeSinceLastGeneration = 0f;
            }
        }
    }

    void Generation()
    {
        GenerateBioms();
        GenerateGrass();
        //worldSorting.SortAll();
    }

    void GenerateGrass()
    {
        foreach(var tile in spawnedTiles)
        {
            var grassGenerator = tile.GetComponent<GrassGenerator>();
            if(grassGenerator != null)
            {
                grassGenerator.GenerateGrass();
            }
        }
    }

    void GenerateBioms()
    {
        float[,] heightMap = GenerateHeightMap();

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float heightValue = heightMap[x, y];
                GameObject tilePrefab = ChooseTilePrefab(heightValue);

                // Create the tile game object
                GameObject tile = Instantiate(tilePrefab, transform);

                // Set the tile's position
                float xPos = x * tileSize;
                float yPos = y * tileSize;
                tile.transform.position = new Vector3(xPos, yPos, 0);

                spawnedTiles.Add(tile);
            }
        }

        chunkManager.enabled = enableChunkManager;
    }

    void ClearTiles()
    {
        Transform[] childTransforms = GetComponentsInChildren<Transform>();
        foreach (Transform child in childTransforms)
        {
            if (child != transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    float[,] GenerateHeightMap()
    {
        float[,] heightMap = new float[mapWidth, mapHeight];

        if (useRandomSeed)
        {
            seed = Random.Range(0, 100000);
        }

        Random.InitState(seed);

        float xOffset = Random.Range(-10000f, 10000f);
        float yOffset = Random.Range(-10000f, 10000f);

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float xCoord = (float)x / mapWidth * heightScale + xOffset;
                float yCoord = (float)y / mapHeight * heightScale + yOffset;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                heightMap[x, y] = sample;
            }
        }

        return heightMap;
    }

    GameObject ChooseTilePrefab(float heightValue)
    {
        for (int i = 0; i < heightThresholds.Length; i++)
        {
            if (heightValue <= heightThresholds[i])
            {
                return specialPrefabs[i];
            }
        }

        return tilePrefabs[tilePrefabs.Length - 1];
    }
}