using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dumpster : MonoBehaviour
{
    [Tooltip("Seconds before an object is destroyed in the trash can")]
    public float destroyDelay = 2f;

    private Dictionary<GameObject, Coroutine> destroyTimers = new();

    private Jiggle jiggle; //the new code that handle jiggle

    private void Start()
    {
        jiggle = GetComponent<Jiggle>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!destroyTimers.ContainsKey(other.gameObject))
        {
            Coroutine timer = StartCoroutine(DestroyAfterDelay(other.gameObject));
            destroyTimers.Add(other.gameObject, timer);

            jiggle.StartJiggle(); //Here is where jiggle start
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