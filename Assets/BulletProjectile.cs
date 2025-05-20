using UnityEngine;

public class BulletProjectile : MonoBehaviour
{

    public Vector3 m_direction;
    public Vector3 m_spawnPos;
    public float m_angle;

    public float m_velocity;
    
    public int bulletDamage;

    public float colliderSize = 0.7f;

    bool hitDetected = false;

    public void setDirection(Vector3 normalizedDirection, float x, float y)
    {
        m_direction = normalizedDirection;
    }

    void Start()
    {
    }

    void Update()
    {
        transform.position += m_direction * m_velocity * Time.deltaTime;

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
