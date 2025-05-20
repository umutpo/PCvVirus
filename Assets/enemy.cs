using System;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.TextCore.Text;

public class Enemy : MonoBehaviour
{

    public Transform targetDest;
    public float velocity;
    public float m_rotationSpeed = 2.0f;
    public float m_currentAngleRelativeToTargetDeg;
    public Rigidbody2D rb;

    public GameObject targetGameObject;
    public Character targetCharacter;

    public int hp = 10;
    public int collisionDamage = 1;

    public float m_targetMaxRadiusToProtect;
    public float m_targetMinRadiusToProtect;

    public float m_distanceToTarget;

    bool m_isOrbiting = false;

    private void Attack()
    {
        if (targetCharacter == null)
        {
            targetCharacter = targetGameObject.GetComponent<Character>();
        }

        targetCharacter.TakeDamage(collisionDamage);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void setTarget(GameObject target) {
        targetGameObject = target;
        targetDest = target.transform;
    }

    public void takeDamage(int damage)
    {
        hp -= damage;

        if (hp < 1)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject == targetGameObject)
        {
            Attack();
        }
    }

    void Start()
    {

    }

    void Update()
    {
        
    }

    void FixedUpdate()
    {

        Vector3 directionToMove = Vector3.zero;

        if (targetGameObject != null)
        {
            m_distanceToTarget = Vector3.Distance(transform.position, targetGameObject.transform.position);

            if (m_distanceToTarget > m_targetMaxRadiusToProtect)
            {
                m_isOrbiting = false;

                // accelerate towards the target
                directionToMove = (targetDest.position - transform.position).normalized;
                rb.linearVelocity = directionToMove * velocity;
            }

            else
            {
                // entering the orbit range, set the initial direction
                if (m_isOrbiting == false)
                {
                    Vector3 direction = (transform.position - targetDest.position).normalized;
                    m_currentAngleRelativeToTargetDeg = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    m_isOrbiting = true;
                }
                
                else
                {
                    // otherwise increment the angle by rotationSpeed
                    m_currentAngleRelativeToTargetDeg += m_rotationSpeed * Time.fixedDeltaTime;
                    float angleRad = m_currentAngleRelativeToTargetDeg * Mathf.Deg2Rad;

                    Vector3 orbitPosition = targetDest.position + new Vector3(
                        Mathf.Cos(angleRad) * m_targetMinRadiusToProtect,
                        Mathf.Sin(angleRad) * m_targetMinRadiusToProtect,
                        0f
                    );

                    rb.linearVelocity = (orbitPosition - transform.position) / Time.fixedDeltaTime;
                }
            }
        }
        else {
            directionToMove = Vector3.zero;
        }
    }
}
