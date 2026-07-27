using UnityEngine;

public sealed class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField, Range(0f, 1f)] private float smoothSpeed = 0.001f;
    [SerializeField] private Vector3 offset;
    [SerializeField, Min(0f)] private float mouseMoveSpeed = 0.2f;

    private Camera controlledCamera;
    private float staticZPosition;

    private void Awake()
    {
        controlledCamera = GetComponent<Camera>();
        if (controlledCamera == null)
        {
            controlledCamera = Camera.main;
        }

        staticZPosition = transform.position.z;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + offset;
        desiredPosition.z = staticZPosition;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        if (!Input.GetMouseButton(1) || controlledCamera == null)
        {
            return;
        }

        Vector3 mouseWorldPosition = controlledCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = staticZPosition;

        Vector3 moveDirection = mouseWorldPosition - transform.position;
        moveDirection.z = 0f;

        if (moveDirection.sqrMagnitude > Mathf.Epsilon)
        {
            transform.position += moveDirection.normalized * mouseMoveSpeed;
            Vector3 position = transform.position;
            position.z = staticZPosition;
            transform.position = position;
        }
    }
}
