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

    private TrashBag currentBag;
    private Queue<GameObject> bagContents = new();
    private Coroutine addRoutine;
    private bool alreadyFull = false;

    private void Start()
    {
        SpawnNewBag();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!alreadyFull)
        {
            if (other.GetComponent<TrashBag>() != null)
                return; // Don't trash a trash bag

            if (addRoutine == null)
                addRoutine = StartCoroutine(AddToBagWithDelay(other.gameObject));
        }
    }

    private IEnumerator AddToBagWithDelay(GameObject obj)
    {
        yield return new WaitForSeconds(addToBagDelay);

        if (currentBag == null)
            yield break;

        obj.transform.SetParent(currentBag.transform);
        obj.SetActive(false); // Hide inside the bag
        bagContents.Enqueue(obj);

        if (bagContents.Count >= bagCapacity)
        {
            alreadyFull = true;
        }
        else
        {
            alreadyFull = false;
        }

        addRoutine = null;
    }

    private void SpawnNewBag()
    {
        GameObject newBagObj = Instantiate(trashBagPrefab, bagParent);
        newBagObj.transform.localPosition = Vector3.zero;
        newBagObj.SetActive(false); // hidden initially

        currentBag = newBagObj.GetComponent<TrashBag>();
        currentBag.gameObject.SetActive(false);
    }

    public void TakeOutTrash()
    {
        if (currentBag != null && bagContents.Count > 0) // the trash should at least have something
        {
            currentBag.transform.SetParent(null);
            currentBag.gameObject.SetActive(true); // reveal the bag
            bagContents.Clear();
            currentBag = null;
        }
        else
        {
            return; // ignore it
        }
        SpawnNewBag();
    }
}