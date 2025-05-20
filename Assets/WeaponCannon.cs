using System;
using System.Numerics;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class WeaponCannon : MonoBehaviour
{

    public float autoAttackTimer = 1.0f;
    public float bulletSpawnTimer; 

    public Vector3 mouseWorldPosition;
    public Vector2 mouseScreenPosition;
    public Vector3 normalizedMouseDireciton;
    public Vector2 moveOffset;
    public float angle;
    public float distanceFromPlayer = 1.0f;

    public Transform playerTransform;
    private Camera mainCamera;

    // private SpriteRenderer sr;

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
        normalizedMouseDireciton = (mouseWorldPosition - playerTransform.position).normalized;
        
        // reticule
        rotateTowardsCursor();
        moveAroundPlayer();
        
        // bullets
        if (bulletSpawnTimer < autoAttackTimer)
        {
            bulletSpawnTimer += Time.deltaTime;
            return;
        }

        bulletSpawnTimer = 0;
        spawnBullet();
    }

    private void spawnBullet()
    {
        GameObject bullet = Instantiate(bulletPreFab);
        bullet.transform.rotation = getRotationForNewSpawn();
        bullet.transform.position = getPositionForNewSpawn(bullet);
        
        bullet.GetComponent<BulletProjectile>().setDirection(normalizedMouseDireciton, bullet.transform.position.x, bullet.transform.position.y);
        bullet.SetActive(true);
    }

    private Vector3 getPositionForNewSpawn(GameObject bullet)
    {
        float bulletAngleInRadians = bullet.transform.rotation.eulerAngles.z * Mathf.Deg2Rad;

        float x = distanceFromPlayer * Mathf.Cos(bulletAngleInRadians);
        float y = distanceFromPlayer * Mathf.Sin(bulletAngleInRadians);

        Vector3 bulletPos = new Vector3(x, y, 0);
        return playerTransform.position + bulletPos;
    }

    private Quaternion getRotationForNewSpawn()
    {
        float newSpawnAngle = Mathf.Atan2(normalizedMouseDireciton.y, normalizedMouseDireciton.x) * Mathf.Rad2Deg;
        return Quaternion.Euler(new Vector3(0,0,newSpawnAngle));
    }

    void FixedUpdate()
    {
    }

    private void rotateTowardsCursor()
    {
        angle = Mathf.Atan2(normalizedMouseDireciton.y, normalizedMouseDireciton.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }



    private void moveAroundPlayer()
    {
        // use the angle instead of this unit vector
        float angleInRadians = angle * Mathf.Deg2Rad;
        float x = distanceFromPlayer * Mathf.Cos(angleInRadians);
        float y = distanceFromPlayer * Mathf.Sin(angleInRadians);

        Vector3 reticulePos = new Vector3(x, y, 0);

        transform.position = playerTransform.position + reticulePos;
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
