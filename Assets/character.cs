using System;
using UnityEngine;

public class Character : MonoBehaviour
{

public int maxHp = 100;
public int currentHp = 100;


public statusbar hpBar;

    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        currentHp = Math.Max(0, currentHp);

        if (currentHp <= 0) {
            Debug.Log("You Died.");
        }

        hpBar.setState(currentHp, maxHp);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
