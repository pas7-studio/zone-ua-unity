using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassGenerator : MonoBehaviour
{
    public GameObject[] grassPrefabs;
    public float grassScaleMin = 0.5f;
    public float grassScaleMax = 1.5f;
    public int minNumGrassPrefabs = 0;
    public int maxNumGrassPrefabs = 5;
    public float zeroCoefficient = 0.5f;

    public int rotateYMin = -15;
    public int rotateYMax = 15;

    public void GenerateGrass()
    {
        // get bounds of tile
        Bounds bounds = GetComponent<Renderer>().bounds;

        // get grass sorting script
        // GrassSorting grassSorting = GetComponentInChildren<GrassSorting>();

        // generate random number of grass prefabs
        var numGrassPrefabs = Random.Range(minNumGrassPrefabs, maxNumGrassPrefabs + 1);
        if (minNumGrassPrefabs == 0 && Random.value > zeroCoefficient)
        {
            numGrassPrefabs = Random.Range(1, maxNumGrassPrefabs + 1);
        }
        else if(minNumGrassPrefabs == 0)
        {
            numGrassPrefabs = 0;
        }

        for (int i = 0; i < numGrassPrefabs; i++)
        {
            // choose random grass prefab
            GameObject grassPrefab = grassPrefabs[Random.Range(0, grassPrefabs.Length)];

            // generate random position and rotation within bounds
            Vector2 pos = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y)
            );
            Quaternion rot = Quaternion.Euler(0, Random.Range(rotateYMin, rotateYMax), 0);

            // generate random scale
            float scale = Random.Range(grassScaleMin, grassScaleMax);

            // instantiate grass prefab with randomized position, rotation, and scale
            GameObject grass = Instantiate(grassPrefab, pos, rot, transform);
            grass.transform.localScale = new Vector3(scale, scale, 0);


            // add grass sprite renderer to grass sorting script
            // SpriteRenderer spriteRenderer = grass.GetComponentInChildren<SpriteRenderer>();
            //grassSorting.grassSprites.Add(spriteRenderer);
        }
    }
}


