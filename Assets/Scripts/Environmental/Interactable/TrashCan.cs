using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCan : MonoBehaviour
{
    [Tooltip("Seconds before an object is added to the trash bag")]
    public float addToBagDelay = 2f;

    [Tooltip("Prefab of the Trash Bag")]
    public GameObject trashBagPrefab;

    [Tooltip("Where the new trash bag should spawn under this can")]
    public Transform bagParent;

    [Tooltip("Max number of items the trash bag can hold before taking out")]
    public int bagCapacity = 5;

    public GameObject emptyTrashCan;
    public GameObject fullTrashCan;

    private TrashBag currentBag;
    private int currentCount = 0;
    private bool alreadyFull = false;
    private HashSet<GameObject> overlappingItems = new();

    private Jiggle jiggle; //the new code that handle jiggle

    // Cache layer masks for performance
    private static int p2p3RangeLayer = -1;
    private static int p2p3ArrowLayer = -1;

    // Cache WaitForSeconds to avoid allocation
    private WaitForSeconds bagDelayWait;

    private void Awake()
    {
        // Initialize cached layer masks only once
        if (p2p3RangeLayer == -1)
        {
            p2p3RangeLayer = LayerMask.NameToLayer("P2 & P3 Range");
            p2p3ArrowLayer = LayerMask.NameToLayer("P2 & P3 Arrow");
        }

        // Cache WaitForSeconds to avoid allocation in coroutine
        bagDelayWait = new WaitForSeconds(addToBagDelay);
    }

    private void Start()
    {
        if (alreadyFull)
        {
            emptyTrashCan.SetActive(false);
            fullTrashCan.SetActive(true);
        }
        SpawnNewBag();

        jiggle = GetComponent<Jiggle>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Early exit if already full
        if (alreadyFull) return;

        // Check layer first (fastest check)
        int layer = other.gameObject.layer;
        if (layer == p2p3RangeLayer || layer == p2p3ArrowLayer) return;

        // Use TryGetComponent for better performance and null safety
        if (other.TryGetComponent<TrashBag>(out _) || 
            other.TryGetComponent<KnifeController>(out _) || 
            other.TryGetComponent<FirearmController>(out _) || 
            other.TryGetComponent<PlateSystem>(out _)) return;

        GameObject obj = other.gameObject;

        if (overlappingItems.Add(obj))
        {
            StartCoroutine(AddToBagWithDelay(obj));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        overlappingItems.Remove(other.gameObject);
    }

    private IEnumerator AddToBagWithDelay(GameObject obj)
    {
        yield return bagDelayWait; // Use cached WaitForSeconds

        if (currentBag == null || obj == null || !overlappingItems.Contains(obj)) yield break;

        obj.transform.SetParent(currentBag.transform);
        obj.SetActive(false);

        overlappingItems.Remove(obj);
        currentCount++;
        alreadyFull = currentCount >= bagCapacity;

        jiggle.StartJiggle(); //Here is where jiggle start

        if (alreadyFull)
        {
            emptyTrashCan.SetActive(false);
            fullTrashCan.SetActive(true);
        }
    }

    private void SpawnNewBag()
    {
        GameObject newBagObj = Instantiate(trashBagPrefab, bagParent);
        newBagObj.transform.localPosition = Vector3.zero;
        newBagObj.SetActive(false);

        currentBag = newBagObj.GetComponent<TrashBag>();
        currentBag.gameObject.SetActive(false);

        emptyTrashCan.SetActive(true);
        fullTrashCan.SetActive(false);
    }

    public void TakeOutTrash()
    {
        if (currentBag != null && currentCount > 0)
        {
            currentBag.transform.SetParent(null);
            currentBag.gameObject.SetActive(true);

            currentCount = 0;
            alreadyFull = false;
            currentBag = null;

            overlappingItems.Clear();
            SpawnNewBag();
        }
    }
}