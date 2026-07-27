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
    private Vector2 lookInput;
    private bool lookInputIsScreenPosition = true;
    private bool sprintRequested;

    public float CurrentSpeed => currentSpeed;
    public Vector2 MovementInput => movementInput;
    public bool SprintRequested => sprintRequested;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
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

        SetAnimatorSpeed(currentSpeed);
    }

    public void SetMovementInput(Vector2 input)
    {
        movementInput = Vector2.ClampMagnitude(input, 1f);
    }

    public void SetSprintRequested(bool requested)
    {
        sprintRequested = requested;
    }

    public void SetLookInput(Vector2 input, bool isScreenPosition)
    {
        lookInput = input;
        lookInputIsScreenPosition = isScreenPosition;
    }

    public void ClearInput()
    {
        movementInput = Vector2.zero;
        lookInput = Vector2.zero;
        sprintRequested = false;
        currentSpeed = 0f;
        SetAnimatorSpeed(0f);
    }

    private void UpdateFacing()
    {
        if (lookInput.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Vector3 targetWorldPosition;
        if (lookInputIsScreenPosition)
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mainCamera == null)
            {
                return;
            }

            targetWorldPosition = mainCamera.ScreenToWorldPoint(lookInput);
        }
        else
        {
            targetWorldPosition = transform.position + (Vector3)lookInput;
        }

        bool shouldFaceLeft = targetWorldPosition.x <= transform.position.x;
        float targetYRotation = shouldFaceLeft ? 180f : 0f;
        if (!Mathf.Approximately(transform.eulerAngles.y, targetYRotation))
        {
            transform.rotation = Quaternion.Euler(0f, targetYRotation, 0f);
        }
    }

    private void SetAnimatorSpeed(float value)
    {
        if (animator != null && animator.isActiveAndEnabled && animator.isInitialized && animator.runtimeAnimatorController != null)
            animator.SetFloat(SpeedHash, value);
    }

    private void OnDisable()
    {
        ClearInput();
    }

    private void OnValidate()
    {
        speed = Mathf.Max(0f, speed);
        runSpeed = Mathf.Max(speed, runSpeed);
    }
}
