using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCan : MonoBehaviour
{
    [Tooltip("Seconds before an object is destroyed in the trash can")]
    public float destroyDelay = 2f;

    private Dictionary<GameObject, Coroutine> destroyTimers = new();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!destroyTimers.ContainsKey(other.gameObject))
        {
            Coroutine timer = StartCoroutine(DestroyAfterDelay(other.gameObject));
            destroyTimers.Add(other.gameObject, timer);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (destroyTimers.TryGetValue(other.gameObject, out Coroutine timer))
        {
            StopCoroutine(timer);
            destroyTimers.Remove(other.gameObject);
        }
    }

    private IEnumerator DestroyAfterDelay(GameObject obj)
    {
        yield return new WaitForSeconds(destroyDelay);

        if (obj != null)
        {
            destroyTimers.Remove(obj);
            Destroy(obj);
        }
    }
}