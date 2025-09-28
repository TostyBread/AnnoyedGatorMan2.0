using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dumpster : MonoBehaviour
{
    [Tooltip("Seconds before an object is destroyed in the trash can")]
    public float destroyDelay = 2f;

    private Dictionary<GameObject, Coroutine> destroyTimers = new();

    private Jiggle jiggle; //the new code that handle jiggle

    // Cache layer masks for performance
    private static int p2p3RangeLayer = -1;
    private static int p2p3ArrowLayer = -1;

    // Cache WaitForSeconds to avoid allocation
    private WaitForSeconds destroyDelayWait;

    private void Awake()
    {
        // Initialize cached layer masks only once
        if (p2p3RangeLayer == -1)
        {
            p2p3RangeLayer = LayerMask.NameToLayer("P2 & P3 Range");
            p2p3ArrowLayer = LayerMask.NameToLayer("P2 & P3 Arrow");
        }

        // Cache WaitForSeconds to avoid allocation in coroutine
        destroyDelayWait = new WaitForSeconds(destroyDelay);
    }

    private void Start()
    {
        jiggle = GetComponent<Jiggle>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check layer first (fastest check)
        int layer = other.gameObject.layer;
        if (layer == p2p3RangeLayer || layer == p2p3ArrowLayer) return;

        // Use TryGetComponent for better performance
        if (other.TryGetComponent<PlateSystem>(out _)) return;

        GameObject obj = other.gameObject;

        if (!destroyTimers.ContainsKey(obj))
        {
            Coroutine timer = StartCoroutine(DestroyAfterDelay(obj));
            destroyTimers.Add(obj, timer);

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
        yield return destroyDelayWait; // Use cached WaitForSeconds

        if (obj != null && destroyTimers.ContainsKey(obj))
        {
            destroyTimers.Remove(obj);
            Destroy(obj);
        }
    }
}