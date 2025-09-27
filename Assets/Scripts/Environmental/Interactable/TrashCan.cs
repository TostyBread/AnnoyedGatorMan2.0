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
        // Ignore if already full or if the object is another trash bag, knife, firearm, or plate
        if (alreadyFull || other.GetComponent<TrashBag>() != null || other.GetComponent<KnifeController>() != null || other.GetComponent<FirearmController>() != null || other.GetComponent<PlateSystem>() != null) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("P2 & P3 Range") || 
            other.gameObject.layer == LayerMask.NameToLayer("P2 & P3 Arrow")) return; // Ignore P2 & P3 range and arrow objects (they serve as UI)

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
        yield return new WaitForSeconds(addToBagDelay);

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