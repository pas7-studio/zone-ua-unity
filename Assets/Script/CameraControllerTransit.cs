using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject target = null; // The object that the camera will follow
    [SerializeField] private Vector3 offset; // The offset of the camera from the target object
    [SerializeField] private float smoothSpeed = 0.125f; // The speed at which the camera will move towards its target position

    private CameraPointsController pointsController;
    private Vector3 targetPosition; // The position that the camera will move towards

    private void Start()
    {
        pointsController = this.GetComponent<CameraPointsController>();
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
            transform.position = smoothedPosition;

            if (HasReachedTargetPosition())
            {
                target = null;
            }
        }
    }

    // Call this function to set the camera's target position to a new position
    public void SetTargetPosition(string name)
    {
        target = pointsController.getPointByName(name);
        targetPosition = target.transform.position + offset;
    }

    private bool HasReachedTargetPosition(float threshold = 0.1f)
    {
        // Calculate the distance between the current position of the GameObject and the target position
        float distanceToTarget = Vector3.Distance(this.transform.position, targetPosition);

        // Check if the distance is less than the threshold (i.e. if the GameObject has reached the target position)
        if (distanceToTarget < threshold)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}