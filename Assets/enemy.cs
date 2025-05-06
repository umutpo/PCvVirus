using UnityEngine;

public class enemy : MonoBehaviour
{

    public Transform targetDest;
    public float velocity;

    public Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
