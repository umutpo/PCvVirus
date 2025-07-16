using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform m_targetDest;
    public float velocity;
    public float m_rotationSpeed = 2.0f;
    public Rigidbody2D m_rb;

    public GameObject m_defenseTarget;
    public GameObject m_attackTarget;
    public Character m_targetCharacter;

    public int m_hp = 10;
    public int m_collisionDamage = 1;

    public PathingType m_pathingType { set; get; }
    public float m_distanceToTarget;

    private Vector3 m_designatedRadialPosition;
    private bool m_hasDesignatedPosition = false;
    public float m_movementSpeed = 5.0f;

    public SwarmController m_swarmController;
    public int m_pathIndex; // index in the radial path
    public int m_indexInPath; // 

    // sprite
    public Vector2 m_spriteSize;
    public float m_ppu;

    private void Start()
    {

    }

    public void setSpriteSize()
    {
        SpriteRenderer enemySpriteRenderer = GetComponent<SpriteRenderer>();
        if (enemySpriteRenderer != null && enemySpriteRenderer.sprite != null)
        {
            m_spriteSize = enemySpriteRenderer.sprite.rect.size;
            m_ppu = enemySpriteRenderer.sprite.pixelsPerUnit;
        }
    }

    private void Attack()
    {
        if (m_targetCharacter == null && m_defenseTarget != null)
        {
            m_targetCharacter = m_attackTarget.GetComponent<Character>();
        }

        if (m_targetCharacter != null)
        {
            Debug.Log("Attacking target: " + m_targetCharacter.name);
            m_targetCharacter.TakeDamage(m_collisionDamage);
        }
    }

    private void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();
    }

    public void setDefenseTarget(GameObject defenseTarget)
    {
        m_defenseTarget = defenseTarget;
        m_targetDest = defenseTarget.transform;
    }

    public void setAttackTarget(GameObject attackTarget)
    {
        m_attackTarget = attackTarget;
        m_targetCharacter = attackTarget.GetComponent<Character>();
    }

    public void setPathingType(PathingType type)
    {
        m_pathingType = type;
    }

    public void setDesignatedRadialPosition(Vector3 position)
    {
        m_designatedRadialPosition = position;
        m_hasDesignatedPosition = true;
    }

    public void takeDamage(int damage)
    {
        m_hp -= damage;

        if (m_hp < 1)
        {
            Destroy(gameObject);
        }
    }

    public void moveTowardsTarget()
    {
        if (m_targetDest != null)
        {
            Vector3 directionToMove = (m_targetDest.position - transform.position).normalized;
            m_rb.linearVelocity = directionToMove * velocity;
        }
        else
        {
            m_rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject == m_attackTarget)
        {
            Attack();
        }
    }

    void FixedUpdate()
    {
        if (m_defenseTarget != null)
        {
            m_distanceToTarget = Vector3.Distance(transform.position, m_defenseTarget.transform.position);

            switch (m_pathingType)
            {
                case PathingType.Target:
                    moveTowardsTarget();
                    break;

                case PathingType.Radial:

                    // I need a smoother entrance into the orbit, and I would like the orbit to have 
                    // some structure to it, like when ants move, you can see their structure and
                    // "descipline" 
                    // but how...?

                    if (m_swarmController != null)
                    {
                        m_designatedRadialPosition = m_swarmController.GetCurrentDesignatedPosition(m_pathIndex, m_indexInPath);
                        m_hasDesignatedPosition = true;
                    }

                    if (m_hasDesignatedPosition)
                    {
                        float distanceToDesignated = Vector3.Distance(transform.position, m_designatedRadialPosition);
                        if (distanceToDesignated < 0.1f)
                        {
                            m_rb.linearVelocity = Vector2.zero;

                            if (m_defenseTarget != null)
                            {
                                Vector3 directionToTarget = (m_defenseTarget.transform.position - transform.position).normalized;
                                float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
                                Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle + 90f);
                                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, m_rotationSpeed * Time.fixedDeltaTime);
                            }
                        }

                        else
                        {
                            Vector3 directionToDesignated = (m_designatedRadialPosition - transform.position).normalized;
                            m_rb.linearVelocity = directionToDesignated * m_movementSpeed;


                            if (m_defenseTarget != null)
                            {
                                Vector3 directionToTarget = (m_defenseTarget.transform.position - transform.position).normalized;
                                float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
                                Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle + 90f);
                                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, m_rotationSpeed * Time.fixedDeltaTime);
                            }
                        }

                        // // old orbit code
                        //  Vector3 orbitPosition = targetDest.position + new Vector3(
                        //  Mathf.Cos(angleRad) * m_targetMinRadiusToProtect,
                        //  Mathf.Sin(angleRad) * m_targetMinRadiusToProtect,
                        //     0f
                        //  );

                        //  rb.linearVelocity = (orbitPosition - transform.position) / Time.fixedDeltaTime;
                    }
                    else
                    {
                        moveTowardsTarget();
                    }
                    break;
            }
        }
        else
        {
            Debug.Log("No target found, stopping movement.");
            m_rb.linearVelocity = Vector2.zero;
        }
    }
}