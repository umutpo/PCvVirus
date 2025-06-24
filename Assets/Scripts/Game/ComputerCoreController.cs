using UnityEngine;

public class ComputerCoreController : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    private void Start()
    {
        currentHealth = maxHealth;
    }
    public void takeDamage(int bulletDamage)
    {
         currentHealth -= bulletDamage;
         currentHealth = Mathf.Max(currentHealth, 0);
         this.gameObject.transform.GetChild(0).GetComponent<statusbar>().setState(currentHealth, maxHealth);
        
        if (currentHealth == 0)
            Destroy(this.gameObject);
    }
}
