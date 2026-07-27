using UnityEngine;

[RequireComponent(typeof(Renderer))]
public sealed class GrassGenerator : MonoBehaviour
{
    [SerializeField] private GameObject[] grassPrefabs;
    [SerializeField, Min(0f)] private float grassScaleMin = 0.5f;
    [SerializeField, Min(0f)] private float grassScaleMax = 1.5f;
    [SerializeField, Min(0)] private int minNumGrassPrefabs;
    [SerializeField, Min(0)] private int maxNumGrassPrefabs = 5;
    [SerializeField, Range(0f, 1f)] private float zeroCoefficient = 0.5f;
    [SerializeField] private int rotateYMin = -15;
    [SerializeField] private int rotateYMax = 15;

    private Renderer cachedRenderer;

    private void Awake()
    {
        cachedRenderer = GetComponent<Renderer>();
    }

    public void GenerateGrass()
    {
        if (grassPrefabs == null || grassPrefabs.Length == 0)
        {
            return;
        }

        cachedRenderer ??= GetComponent<Renderer>();
        Bounds bounds = cachedRenderer.bounds;

        int count = Random.Range(minNumGrassPrefabs, maxNumGrassPrefabs + 1);
        if (minNumGrassPrefabs == 0)
        {
            count = Random.value <= zeroCoefficient
                ? 0
                : Random.Range(1, maxNumGrassPrefabs + 1);
        }

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = grassPrefabs[Random.Range(0, grassPrefabs.Length)];
            if (prefab == null)
            {
                continue;
            }

            Vector2 position = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y));

            Quaternion rotation = Quaternion.Euler(
                0f,
                Random.Range(rotateYMin, rotateYMax),
                0f);

            float scale = Random.Range(grassScaleMin, grassScaleMax);
            GameObject grass = Instantiate(prefab, position, rotation, transform);
            grass.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }

    private void OnValidate()
    {
        grassScaleMax = Mathf.Max(grassScaleMin, grassScaleMax);
        maxNumGrassPrefabs = Mathf.Max(minNumGrassPrefabs, maxNumGrassPrefabs);

        if (rotateYMax < rotateYMin)
        {
            rotateYMax = rotateYMin;
        }
    }
}
