using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class CharacterCustomController : MonoBehaviour
{
    public float currentSpeed = 0f;
    public float speed = 5.0f;
    public float runSpeed = 10.0f;

    private Rigidbody2D rb2d;
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (horizontal != 0f || vertical != 0f)
        {

            Vector2 movement = new Vector2(horizontal, vertical);
            movement.Normalize();

            if (Input.GetKey(KeyCode.LeftShift))
            {
                currentSpeed = runSpeed;
                rb2d.MovePosition(rb2d.position + movement * runSpeed * Time.fixedDeltaTime);
            }
            else
            {
                currentSpeed = speed;
                rb2d.MovePosition(rb2d.position + movement * speed * Time.fixedDeltaTime);
            }
        }
        else
        {
            currentSpeed = 0f;
        }

        anim.SetFloat("Speed", currentSpeed);
    }

    private void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.nearClipPlane;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        Vector3 direction = worldPos - transform.position;

        if (direction.x > 0 && transform.rotation.y == 1f)
        {
            transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        }
        else if(direction.x <= 0 && transform.rotation.y == 0f)
        {
            transform.rotation = new Quaternion(0f, 180f, 0f, 0f);
        }
    }

}