using UnityEngine;

public class BulletProjectile : MonoBehaviour
{

    public Vector3 direction;
    public float velocity; 
    
    public int bulletDamage;

    public float colliderSize = 0.7f;

    bool hitDetected = false;

    public void setDirection(float x, float y) {
        direction = new Vector3(x, y);
        
        
        // if (x < 0) {
        //     Vector3 scale = transform.localScale;
        //     scale.x = scale.x * -1;
        //     transform.localScale = scale;
        // }
    }

    void Start()
    {
        
    }

    void Update()
    {
        transform.position += direction * velocity * Time.deltaTime;

        Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position, colliderSize);

        foreach(Collider2D c in hit) {
            Enemy enemy = c.GetComponent<Enemy>();
            if (enemy != null) {

                enemy.takeDamage(bulletDamage);
                hitDetected = true;
                break;
            }
        }

        if (hitDetected == true) {
            Destroy(gameObject);
        }
    }
}
