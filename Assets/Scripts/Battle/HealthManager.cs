using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public float Health = 20;
    private float currentHealth;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = Health;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentHealth <= 0) 
        {
            currentHealth = 0;
            Destroy(gameObject);
        }
    }

    public void TryDamage(float Damage)
    {
        currentHealth -= Damage;
    }
}
