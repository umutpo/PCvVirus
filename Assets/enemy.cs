using System;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.TextCore.Text;

public class Enemy : MonoBehaviour
{

    public Transform targetDest;
    public float velocity;
    public float m_rotationSpeed = 5.0f;
    public float m_currentAngleRelativeToTarget;
    public Rigidbody2D rb;

    public GameObject targetGameObject;
    public Character targetCharacter;

    public int hp = 10;
    public int collisionDamage = 1;

    public float m_targetMaxRadiusToProtect;
    public float m_targetMinRadiusToProtect;

    public float m_distanceToTarget;

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
            m_currentAngleRelativeToTarget = Vector3.Angle(transform.position, targetDest.position);
            Debug.DrawLine(Vector3.zero, transform.position, Color.red);
            Debug.DrawLine(Vector3.zero, targetDest.position, Color.green);

            if (m_distanceToTarget > m_targetMaxRadiusToProtect)
            {
                // accelerate towards the target
                directionToMove = (targetDest.position - transform.position).normalized;
                rb.linearVelocity = directionToMove * velocity;
            }

            else
            {
                // now start orbiting the target
                // m_currentAngleRelativeToTarget += m_rotationSpeed * Time.deltaTime;


                // Vector3 offset = new Vector3(Mathf.Cos(m_currentAngleRelativeToTarget), Mathf.Sin(m_currentAngleRelativeToTarget), 0);
                // Vector3 newPosition = targetDest.position + offset * m_targetMaxRadiusToProtect;

                // rb.MovePosition(Vector3.MoveTowards(transform.position, newPosition, velocity * Time.deltaTime));
                // Debug.DrawRay(transform.position, newPosition, Color.red);
            }
        }
        else {
            directionToMove = Vector3.zero;
        }
    }
}
