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

    private HashSet<GameObject> objectsInFire = new HashSet<GameObject>();
    private Coroutine heatCoroutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isFireSource) return;

        ItemSystem item = other.GetComponent<ItemSystem>();
        if (item != null && objectsInFire.Add(other.gameObject)) // Only add if not already inside
        {
            if (heatCoroutine == null)
            {
                heatCoroutine = StartCoroutine(ApplyHeatOverTime());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (objectsInFire.Remove(other.gameObject) && objectsInFire.Count == 0)
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