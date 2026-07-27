using UnityEngine;

[RequireComponent(typeof(CameraPointsController))]
public sealed class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private Vector3 offset;
    [SerializeField, Range(0f, 1f)] private float smoothSpeed = 0.125f;
    [SerializeField, Min(0f)] private float arrivalThreshold = 0.1f;

    private CameraPointsController pointsController;
    private Vector3 targetPosition;

    private void Awake()
    {
        pointsController = GetComponent<CameraPointsController>();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);

        if (HasReachedTargetPosition())
        {
            target = null;
        }
    }

    public void SetTargetPosition(string pointName)
    {
        TrySetTargetPosition(pointName);
    }

    public bool TrySetTargetPosition(string pointName)
    {
        GameObject point = pointsController.GetPointByName(pointName);
        if (point == null)
        {
            Debug.LogWarning($"Camera point '{pointName}' was not found.", this);
            return false;
        }

        target = point;
        targetPosition = point.transform.position + offset;
        return true;
    }

    private bool HasReachedTargetPosition()
    {
        float squaredThreshold = arrivalThreshold * arrivalThreshold;
        return (transform.position - targetPosition).sqrMagnitude <= squaredThreshold;
    }
}
