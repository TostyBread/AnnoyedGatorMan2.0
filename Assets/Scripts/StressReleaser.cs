using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StressReleaser: MonoBehaviour
{
    public bool InZone;

    [Header("References")]
    public DetectZone detectZone;
    public Sanity sanity;
    public GameObject explosion;

    private HealthManager healthManager;
    private float currentHealth;

    // Start is called before the first frame update
    void Start()
    {
        healthManager = GetComponent<HealthManager>();
        currentHealth = healthManager.currentHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (detectZone) InZone = detectZone.inDetactZone;

        if (healthManager.currentHealth <= 0)
        {
            GameObject.Instantiate(explosion, transform.position, transform.rotation);
        }

        IncreaseSanity();
    }
    public void IncreaseSanity()
    {
        if (healthManager.currentHealth != currentHealth)
        {
            if (detectZone && detectZone.inDetactZone)
            { 
                sanity.RemainSanity += healthManager.damageReceived;
            }

            if (!detectZone)
            {
                sanity.RemainSanity += healthManager.damageReceived;
            }
            
            currentHealth = healthManager.currentHealth;
        }
    }
}
