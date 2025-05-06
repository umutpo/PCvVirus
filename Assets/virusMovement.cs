using UnityEngine;
using UnityEngine.InputSystem;

public class virusMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    public Vector2 _moveDirection;

    public float frameVelocity;
    public float targetVelocity = 5f;
    public float maxVelocity = 10f;
    public float acceleration = 0.5f;

    [Header("Input Actions")]
    public InputActionReference moveInput;
    public InputActionReference fireInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
    }


    private void OnEnable()
    {
        if (fireInput != null && fireInput.action != null)
        {
            fireInput.action.started += Fire;
        }
        else
        {
            Debug.LogWarning("Fire input action is not set.");
        }

        if (moveInput != null && moveInput.action != null)
        {
            moveInput.action.Enable();
        }
        else
        {
            Debug.LogWarning("Move input action is not set.");
        }
    }

    private void OnDisable()
    {
        if (fireInput != null && fireInput.action != null)
        {
            fireInput.action.started -= Fire;
        }

        if (moveInput != null && moveInput.action != null)
        {
            moveInput.action.Disable();
        }
    }

    void Update()
    {
        if (moveInput != null && moveInput.action != null)
        {
            _moveDirection = moveInput.action.ReadValue<Vector2>();
        }
        else
        {
            Debug.LogWarning("Move input action is not set.");
        }
    }

    private void FixedUpdate()
    {
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        if (rb != null)
        {

            if (_moveDirection == Vector2.zero)
            {
                frameVelocity = 0f;
            }
            if (frameVelocity < targetVelocity)
            {
                frameVelocity += acceleration;
            }
            Mathf.Clamp(frameVelocity, 0, targetVelocity);
            rb.linearVelocity = _moveDirection * frameVelocity;
        }
        else
        {
            Debug.LogError("Rigidbody2D component is not attached.");
        }
    }

    private void Fire(InputAction.CallbackContext obj)
    {
        Debug.Log("Fired");
    }
}
