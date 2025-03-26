using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    public float burnRadius = 2f;
    public float spreadInterval = 3f; 
    public int maxFireInstances = 10; 

    private static int fireCount = 0; 

    void Start()
    {
        fireCount++;
        StartCoroutine(SpreadFire());
    }

    private IEnumerator SpreadFire()
    {
        while (fireCount < maxFireInstances)
        {
            yield return new WaitForSeconds(spreadInterval);

            Vector2 randomOffset = Random.insideUnitCircle * burnRadius;
            Vector2 spawnPosition = (Vector2)transform.position + randomOffset;

            Instantiate(this.gameObject, spawnPosition, Quaternion.identity);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, burnRadius);
    }

    private void OnDestroy()
    {
        fireCount--;
    }
}