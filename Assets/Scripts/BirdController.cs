using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class BirdController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private bool useForwardSpeed = false;
    [SerializeField] private float forwardSpeed = 2.5f;

    [Header("Rotation")]
    [SerializeField] private float maxUpRotation = 45f;
    [SerializeField] private float maxDownRotation = -45f;
    [SerializeField] private float idleDownAngle = -20f;
    [SerializeField] private float rotationSmooth = 12f;
    [SerializeField] private float rotationVelocityMin = -3f;
    [SerializeField] private float rotationVelocityMax = 3f;

    private Rigidbody2D rb;
#if ENABLE_INPUT_SYSTEM
    private InputAction flapAction;
#endif

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;

#if ENABLE_INPUT_SYSTEM
        flapAction = new InputAction(type: InputActionType.Button);
        flapAction.AddBinding("<Keyboard>/space");
        flapAction.AddBinding("<Mouse>/leftButton");
        flapAction.AddBinding("<Pointer>/press");
#endif
    }

    private void OnEnable()
    {
#if ENABLE_INPUT_SYSTEM
        flapAction?.Enable();
#endif
    }

    private void OnDisable()
    {
#if ENABLE_INPUT_SYSTEM
        flapAction?.Disable();
#endif
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            return;
        }

        if (IsFlapPressed())
        {
            Flap();
        }

        RotateByVelocity();
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            return;
        }

        if (useForwardSpeed)
        {
            rb.linearVelocity = new Vector2(forwardSpeed, rb.linearVelocity.y);
        }
    }

    private bool IsFlapPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return flapAction != null && flapAction.WasPressedThisFrame();
#else
        return Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
#endif
    }

    private void Flap()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void RotateByVelocity()
    {
        float vy = rb.linearVelocity.y;
        float t = Mathf.InverseLerp(rotationVelocityMin, rotationVelocityMax, vy);
        float targetZ = Mathf.Lerp(maxDownRotation, maxUpRotation, t);

        if (Mathf.Abs(vy) < 0.15f)
        {
            targetZ = idleDownAngle;
        }

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetZ);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmooth * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsObstacle(collision.collider))
        {
            GameManager.Instance?.GameOver();
        }
    }

    private static bool IsObstacle(Collider2D collider)
    {
        if (collider == null)
        {
            return false;
        }

        Transform current = collider.transform;
        while (current != null)
        {
            if (current.CompareTag("Ground") || current.CompareTag("Pipe"))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }
}
