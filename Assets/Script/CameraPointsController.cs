using System.Collections.Generic;
using UnityEngine;

public sealed class CameraPointsController : MonoBehaviour
{
    [SerializeField] private List<GameObject> pointsList = new List<GameObject>();

    private readonly Dictionary<string, GameObject> pointsByName =
        new Dictionary<string, GameObject>();

    private void Awake()
    {
        RebuildLookup();
    }

    public void RebuildLookup()
    {
        pointsByName.Clear();

        if (pointsList == null || pointsList.Count == 0)
        {
            GameObject[] discoveredPoints = GameObject.FindGameObjectsWithTag("CameraPoint");
            pointsList = new List<GameObject>(discoveredPoints);
        }

        for (int i = 0; i < pointsList.Count; i++)
        {
            GameObject pointObject = pointsList[i];
            if (pointObject == null ||
                !pointObject.TryGetComponent(out CameraPoint point) ||
                string.IsNullOrWhiteSpace(point.PointName))
            {
                continue;
            }

            if (!pointsByName.TryAdd(point.PointName, pointObject))
            {
                Debug.LogWarning($"Duplicate camera point name '{point.PointName}'.", pointObject);
            }
        }
    }

    public GameObject GetPointByName(string pointName)
    {
        if (string.IsNullOrWhiteSpace(pointName))
        {
            return null;
        }

        pointsByName.TryGetValue(pointName, out GameObject point);
        return point;
    }

    // Backwards-compatible method name.
    public GameObject getPointByName(string pointName) => GetPointByName(pointName);
}
