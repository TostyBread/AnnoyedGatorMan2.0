using System.Collections;
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
    private Coroutine addRoutine;
    private bool alreadyFull = false;

    private void Start()
    {
        if (alreadyFull)
        {
            emptyTrashCan.SetActive(false);
            fullTrashCan.SetActive(true);
        }
        SpawnNewBag();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!alreadyFull && addRoutine == null)
        {
            if (other.GetComponent<TrashBag>() != null)
                return;

            addRoutine = StartCoroutine(AddToBagWithDelay(other.gameObject));
        }
    }

    private IEnumerator AddToBagWithDelay(GameObject obj)
    {
        yield return new WaitForSeconds(addToBagDelay);

        if (currentBag == null)
        {
            addRoutine = null;
            yield break;
        }

        obj.transform.SetParent(currentBag.transform);
        obj.SetActive(false);

        currentCount++;
        alreadyFull = currentCount >= bagCapacity;

        addRoutine = null;

        if (currentCount >= bagCapacity)
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

        emptyTrashCan.SetActive(true); // empty trash can sprite activates
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
            SpawnNewBag();
        }
    }
}