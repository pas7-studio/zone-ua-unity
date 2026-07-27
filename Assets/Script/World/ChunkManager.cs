using System.Collections.Generic;
using UnityEngine;

public sealed class ChunkManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject chunkParent;
    [SerializeField, Min(0.02f)] private float updateInterval = 0.5f;
    [SerializeField, Min(0f)] private float buffer = 0.5f;

    private readonly List<ChunkEntry> chunks = new List<ChunkEntry>();
    private readonly Plane[] frustumPlanes = new Plane[6];

    private float updateTimer;

    private sealed class ChunkEntry
    {
        public GameObject GameObject;
        public Renderer Renderer;
    }

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Start()
    {
        RefreshChunks();
        UpdateChunksVisibility();
    }

    private void Update()
    {
        updateTimer += Time.deltaTime;
        if (updateTimer < updateInterval)
        {
            return;
        }

        updateTimer = 0f;
        UpdateChunksVisibility();
    }

    public void RefreshChunks()
    {
        chunks.Clear();

        Transform parent = chunkParent != null ? chunkParent.transform : transform;
        foreach (Transform child in parent)
        {
            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer == null)
            {
                continue;
            }

            chunks.Add(new ChunkEntry
            {
                GameObject = child.gameObject,
                Renderer = renderer
            });
        }
    }

    private void UpdateChunksVisibility()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            return;
        }

        GeometryUtility.CalculateFrustumPlanes(mainCamera, frustumPlanes);

        for (int i = 0; i < frustumPlanes.Length; i++)
        {
            Plane plane = frustumPlanes[i];
            Vector3 point = plane.ClosestPointOnPlane(mainCamera.transform.position);
            frustumPlanes[i] = new Plane(plane.normal, point + plane.normal * buffer);
        }

        for (int i = 0; i < chunks.Count; i++)
        {
            ChunkEntry chunk = chunks[i];
            if (chunk.GameObject == null || chunk.Renderer == null)
            {
                continue;
            }

            bool shouldBeActive =
                GeometryUtility.TestPlanesAABB(frustumPlanes, chunk.Renderer.bounds);

            if (chunk.GameObject.activeSelf != shouldBeActive)
            {
                chunk.GameObject.SetActive(shouldBeActive);
            }
        }
    }

    private void OnValidate()
    {
        updateInterval = Mathf.Max(0.02f, updateInterval);
        buffer = Mathf.Max(0f, buffer);
    }
}
