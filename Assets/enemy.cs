using System;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Enemy : MonoBehaviour
{

    public Transform targetDest;
    public float velocity;

    public Rigidbody2D rb;

    public GameObject targetGameObject;
    public Character targetCharacter;

    public int hp = 10;
    public int collisionDamage = 1;

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
        targetGameObject = targetDest.gameObject;
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
        Vector3 direction = (targetDest.position - transform.position).normalized;
        rb.linearVelocity = direction * velocity;
    }
}
