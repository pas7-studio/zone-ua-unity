using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject chunkParent;
    public float updateInterval = 0.5f;

    private List<GameObject> chunks;

    private void Start()
    {
        // Get a reference to all the chunk game objects
        chunks = new List<GameObject>();
        foreach (Transform child in chunkParent.transform)
        {
            chunks.Add(child.gameObject);
        }

        // Call the UpdateChunksVisibility method every updateInterval seconds
        InvokeRepeating("UpdateChunksVisibility", 0f, updateInterval);
    }

    public float buffer = 0.5f;
    private void UpdateChunksVisibility()
    {
        // Calculate the camera frustum planes with a buffer zone
        
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        for (int i = 0; i < 6; i++)
        {
            Vector3 normal = frustumPlanes[i].normal;
            Vector3 pointOnPlane = frustumPlanes[i].ClosestPointOnPlane(mainCamera.transform.position);
            Vector3 pointWithBuffer = pointOnPlane + normal * buffer;
            frustumPlanes[i] = new Plane(normal, pointWithBuffer);
        }

        // Loop through all the chunks
        foreach (GameObject chunk in chunks)
        {
            // Get the chunk's SpriteRenderer component
            SpriteRenderer spriteRenderer = chunk.GetComponent<SpriteRenderer>();

            // Check if the sprite renderer is null or disabled
            if (spriteRenderer == null || !spriteRenderer.enabled)
            {
                continue;
            }

            // Calculate the chunk's bounds in world space
            Bounds bounds = spriteRenderer.bounds;

            // Check if the chunk is visible to the camera
            bool isVisible = GeometryUtility.TestPlanesAABB(frustumPlanes, bounds);

            // Enable or disable the chunk's game object based on whether it is visible
            chunk.SetActive(isVisible);
        }
    }
}