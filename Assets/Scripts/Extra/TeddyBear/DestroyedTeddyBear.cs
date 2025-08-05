using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyedTeddyBear : MonoBehaviour
{
    public float destroyTime = 5f; // Time before the teddy bear is destroyed

    // Start is called before the first frame update
    void Start()
    {
        DestroyTeddyBear();
    }

    public void DestroyTeddyBear()
    {
        // Start the coroutine to destroy the teddy bear after a delay
        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        // Wait for the specified time
        yield return new WaitForSeconds(destroyTime);
        
        // Destroy the teddy bear game object
        Destroy(gameObject);
    }
}
