using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningKnifeSpawner : MonoBehaviour
{
    public GameObject SpinningKnife;

    public bool isBeingHold = false;
    private bool wasBeingHold = false;

    [Range(0f, 1f)]
    public float spawnChance = 0.5f;

    private Sanity sanity;
    private bool hasSpawned;
    private GameObject parent;

    // Start is called before the first frame update
    void Start()
    {
        sanity = GameObject.FindGameObjectWithTag("Sanity").GetComponent<Sanity>();
    }

    // Update is called once per frame
    void Update()
    {
        if (sanity == null || hasSpawned) 
        {
            return;
        }

        parent = transform.parent != null ? transform.parent.gameObject : null;

        if (parent != null && parent.CompareTag("Player"))
        {
            isBeingHold = true;
        }
        else
        {
            isBeingHold = false;
        }

        if (sanity.RemainSanity <= 0 && !hasSpawned && wasBeingHold && !isBeingHold)
        {

            if (Random.value <= spawnChance)
            {
                Instantiate(SpinningKnife, transform.position, transform.rotation);
                hasSpawned = true;

                Destroy(this.gameObject);
            }
        }

        wasBeingHold = isBeingHold;
    }
}
