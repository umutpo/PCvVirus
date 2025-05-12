using System;
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class WeaponCannon : MonoBehaviour
{

    public float timeToAttack = 1.0f;
    public float timer; 

    public Vector3 mouseWorldPosition;
    public Vector2 mouseScreenPosition;
    public Vector2 direction;
    public Vector2 moveDirection;
    public Vector2 moveOffset;
    public float angle;
    
    public float distanceFromPlayer = 1.0f;

    public Transform playerTransform;
    private Camera mainCamera;

    public int bulletDamage = 5;

    public GameObject bulletPreFab;

    void Start()
    {
        mainCamera = Camera.main;
        playerTransform = transform.parent;
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
        
        if (timer < timeToAttack) {
            timer += Time.deltaTime;
            return;
        }

        timer = 0;
        spawnBullet();
    }

    private void spawnBullet()
    {
        GameObject bullet = Instantiate(bulletPreFab);
        bullet.transform.rotation = getRotationForNewSpawn();
        bullet.transform.position = getPositionForNewSpawn();
        
        bullet.GetComponent<BulletProjectile>().setDirection(bullet.transform.position.x, bullet.transform.position.y);
        bullet.SetActive(true);
        
    }

    private Vector3 getPositionForNewSpawn()
    {
        Vector2 newSpawnMoveDirection = (mouseWorldPosition - playerTransform.position).normalized;
        Vector2 offset = newSpawnMoveDirection * distanceFromPlayer;
        return playerTransform.position + (Vector3)(moveDirection * distanceFromPlayer);
    }

    private Quaternion getRotationForNewSpawn()
    {
        float newSpawnAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return Quaternion.Euler(new Vector3(0,0,newSpawnAngle));
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
        moveDirection = (mouseWorldPosition - playerTransform.position).normalized;
        moveOffset = moveDirection * distanceFromPlayer;
        transform.position = playerTransform.position + (Vector3)(moveDirection + moveOffset);
    }

    private void Attack() 
    {
        // timer = timeToAttack;
        // launch bullet projectiles
        // ApplyDamage(bulletDamage);
    }

    private void ApplyDamage(Collider2D[] colliders) {
         for (int i = 0; i < colliders.Length; ++i) {
            Enemy e = colliders[i].GetComponent<Enemy>();
            if (e != null) {
                 colliders[i].GetComponent<Enemy>().takeDamage(bulletDamage);
            }
        }
    }
}
