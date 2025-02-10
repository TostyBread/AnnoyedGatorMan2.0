using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSource : MonoBehaviour
{
    public ItemSystem.DamageType damageType;
    public int damageAmount;
    public float heatAmount;
    public float heatCooldown = 1.0f; // Time between heat applications
    public bool isFireSource = false; // Toggle to manually define if this is a fire source
    public float minVelocityToDamage = 0f; // Minimum velocity required to inflict damage

    private HashSet<GameObject> objectsInFire = new HashSet<GameObject>();
    private Coroutine heatCoroutine;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isFireSource) return; // Skip impact check for fire sources

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null && rb.velocity.magnitude >= minVelocityToDamage)
        {
            ItemSystem item = collision.collider.GetComponent<ItemSystem>();
            if (item != null)
            {
                item.ApplyCollisionEffect(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isFireSource)
        {
            ItemSystem item = other.GetComponent<ItemSystem>();
            if (item != null && objectsInFire.Add(other.gameObject))
            {
                if (heatCoroutine == null)
                {
                    heatCoroutine = StartCoroutine(ApplyHeatOverTime());
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (isFireSource && objectsInFire.Remove(other.gameObject) && objectsInFire.Count == 0)
        {
            StopCoroutine(heatCoroutine);
            heatCoroutine = null;
        }
    }

    private IEnumerator ApplyHeatOverTime()
    {
        while (objectsInFire.Count > 0)
        {
            foreach (var obj in objectsInFire)
            {
                if (obj != null)
                {
                    ItemSystem item = obj.GetComponent<ItemSystem>();
                    if (item != null)
                    {
                        item.ApplyCollisionEffect(gameObject);
                    }
                }
            }
            yield return new WaitForSeconds(heatCooldown);
        }
        heatCoroutine = null;
    }
}