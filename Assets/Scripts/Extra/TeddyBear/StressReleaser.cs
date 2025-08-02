using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StressReleaser: MonoBehaviour
{
    public bool InZone;

    [Header("References")]
    public DetectZone detectZone;
    public GameObject explosion;

    private bool isSpawned;
    private HealthManager healthManager;
    private float currentHealth;
    private Sanity sanity;

    // Start is called before the first frame update
    void Start()
    {
        healthManager = GetComponent<HealthManager>();
        sanity = GameObject.FindGameObjectWithTag("Sanity").GetComponent<Sanity>();

        currentHealth = healthManager.currentHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (detectZone) InZone = detectZone.inDetactZone;

        if (healthManager.currentHealth <= 0 && !isSpawned)
        {   
            GameObject.Instantiate(explosion, transform.position, Quaternion.identity);
            isSpawned = true;
        }

        if (sanity) IncreaseSanity();
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
