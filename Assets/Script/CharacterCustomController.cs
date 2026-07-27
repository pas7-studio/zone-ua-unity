using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class CharacterCustomController : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    [Header("Movement")]
    [SerializeField, Min(0f)] private float currentSpeed;
    [SerializeField, Min(0f)] private float speed = 5f;
    [SerializeField, Min(0f)] private float runSpeed = 10f;

    private Rigidbody2D body;
    private Animator animator;
    private Camera mainCamera;
    private Vector2 movementInput;
    private bool sprintRequested;

    public float CurrentSpeed => currentSpeed;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        movementInput = Vector2.ClampMagnitude(movementInput, 1f);
        sprintRequested = Input.GetKey(KeyCode.LeftShift);

        UpdateFacing();
    }

    private void FixedUpdate()
    {
        float targetSpeed = movementInput.sqrMagnitude > 0f
            ? (sprintRequested ? runSpeed : speed)
            : 0f;

        currentSpeed = targetSpeed;

        if (targetSpeed > 0f)
        {
            body.MovePosition(body.position + movementInput * targetSpeed * Time.fixedDeltaTime);
        }

        animator.SetFloat(SpeedHash, currentSpeed);
    }

    private void UpdateFacing()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            return;
        }

        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        bool shouldFaceLeft = mouseWorldPosition.x <= transform.position.x;
        float targetYRotation = shouldFaceLeft ? 180f : 0f;

        if (!Mathf.Approximately(transform.eulerAngles.y, targetYRotation))
        {
            transform.rotation = Quaternion.Euler(0f, targetYRotation, 0f);
        }
    }

    private void OnValidate()
    {
        speed = Mathf.Max(0f, speed);
        runSpeed = Mathf.Max(speed, runSpeed);
    }
}
