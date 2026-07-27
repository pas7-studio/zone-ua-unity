using UnityEngine;

public sealed class CameraPoint : MonoBehaviour
{
    [SerializeField] private string pointName;

    public string PointName => pointName;
}
