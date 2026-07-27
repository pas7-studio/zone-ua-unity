using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // the target to follow
    public float smoothSpeed = 0.001f; // the speed at which the camera moves

    public Vector3 offset; // the offset from the target's position
    public float mouseMoveSpeed = 0.2f;    // The speed at which the camera moves towards the mouse direction

    private float staticZPosition = 0f; // The static Z position for the camera

    private void Start()
    {
        staticZPosition = transform.position.z;
    }

    void LateUpdate()
    {
        Vector3 desiredPosition = target.position + offset; // calculate the desired position of the camera
        desiredPosition.z = staticZPosition; // fix the Z-axis of the camera
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed); // smoothly move the camera towards the desired position
        transform.position = smoothedPosition; // set the camera's position to the smoothed position

        // Move the camera towards the mouse direction when the right mouse button is pressed
        if (Input.GetMouseButton(1))
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = staticZPosition;
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            Vector3 moveDirection = (mouseWorldPosition - transform.position).normalized;
            moveDirection.z = staticZPosition;
            transform.position += moveDirection * mouseMoveSpeed;
        }
    }
}