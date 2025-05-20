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

    public float rateOfFire = 0.25f; 
    private float timerSinceLastShot = 0.0f;

    public Vector3 mouseWorldPosition;
    public Vector2 mouseScreenPosition;
    public Vector3 normalizedMouseDireciton;
    public Vector2 moveOffset;
    public float angle;
    public float distanceFromPlayer = 0.5f;

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
        
        timerSinceLastShot += Time.deltaTime;
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

    public void Attack() 
    {
        Debug.Log("TRYING TO SHOOT");
        if (timerSinceLastShot > rateOfFire) {
            spawnBullet();
            timerSinceLastShot = 0.0f;
        }
    }
}
