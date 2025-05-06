using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponCannon : MonoBehaviour
{

    public float timetoAttack = 4.0f;
    public float timer; 

    public Vector3 mouseWorldPosition;
    public Vector2 mouseScreenPosition;
    public Vector2 direction;
    public Vector2 moveDirection;
    public Vector2 moveOffset;
    public float angle;
    
    public float distanceFromPlayer = 1.0f;

    public Transform player;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        player = transform.parent;
    }

    void Update()
    {
        mouseScreenPosition = Mouse.current.position.ReadValue();

        // convert the mouse position from screen space to world space
        mouseWorldPosition = mainCamera.ScreenToWorldPoint(
            new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, mainCamera.nearClipPlane));
        
        // direction from the bullet's current rotation to the mouse
        direction = (mouseWorldPosition - transform.position).normalized;

        rotateTowardsCursor();
        moveAroundPlayer();
        
        timer -= Time.deltaTime;
        if (timer < 0.0f) {
            Attack();
        }
    }

    void FixedUpdate()
    {
    }

    private void rotateTowardsCursor() 
    {
        angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0,0, angle));
    }

    private void moveAroundPlayer() 
    {        
        moveDirection = (mouseWorldPosition - player.position).normalized;
        moveOffset = moveDirection * distanceFromPlayer;
        transform.position = player.position + (Vector3)(moveDirection * distanceFromPlayer);
    }

    private void Attack() 
    {
        timer = timetoAttack;
    }
}
