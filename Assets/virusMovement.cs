using UnityEngine;
using UnityEngine.InputSystem;

public class virusMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    public Vector2 _moveDirection;

    [Header("Movement Settings")]
    public float frameVelocity;
    public float targetVelocity = 5f;
    public float maxVelocity = 10f;
    public float acceleration = 0.5f;
    private bool m_canDash = true;
    private bool m_isDashing = false;
    private float m_dashingPower = 2f;
    private float m_dashingTime = 0.2f;
    private float m_dashingCooldown = 1f;

    [Header("Input Actions")]
    public InputActionReference moveInput;
    public InputActionReference fireInput;
    public InputActionReference dashInput;

    [SerializeField] private float horizontalBound = 10f;
    [SerializeField] private float verticalBound = 10f;

    private GameObject weapon;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        weapon = transform.Find("weapon").gameObject;
    }

    void Start()
    {
    }


    private void OnEnable()
    {
        if (fireInput != null && fireInput.action != null)
        {
            fireInput.action.performed += Fire;
            fireInput.action.Enable();
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
        if (dashInput != null && dashInput.action != null)
        {
            dashInput.action.Enable();
        }
        else
        {
            Debug.LogWarning("Dash input action is not set.");
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
        if (dashInput != null && dashInput.action != null)
        {
            dashInput.action.Disable();
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

        if (dashInput != null && dashInput.action != null)
        {
            if (dashInput.action.triggered)
            {
                m_dash = true;
            }
        }

        gameObject.transform.position = new Vector3(
            Mathf.Clamp(gameObject.transform.position.x, - horizontalBound, horizontalBound),
            Mathf.Clamp(gameObject.transform.position.y, - verticalBound, verticalBound),
            gameObject.transform.position.z
        );
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
        WeaponCannon weaponCannon = weapon.GetComponent<WeaponCannon>();
        weaponCannon.Attack();
    }
}
