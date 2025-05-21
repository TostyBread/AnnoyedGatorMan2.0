using UnityEngine;

public class ItemPackage : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject itemPrefab;
    public int itemCount;
    public bool destroyWhenEmpty = false;
    public Transform spawnPosition;
    public GameObject originalSprite;
    public GameObject emptySprite;
    private int itemAvailability;

    private void Awake()
    {
        if (emptySprite != null)
        {
            originalSprite.SetActive(true);
            emptySprite.SetActive(false);
        }
        itemAvailability = 0;
    }

    public void TakingOutItem()
    {
        if (itemPrefab != null && spawnPosition != null)
        {
            if (itemAvailability != itemCount)
            {
                Instantiate(itemPrefab, spawnPosition.position, spawnPosition.rotation);
                ++itemAvailability;
                PackageEmpty();
                return;
            }
        }
    }

    private void PackageEmpty()
    {
        if(itemAvailability == itemCount && destroyWhenEmpty)
        {
            Destroy(gameObject);
        }
        else if (itemAvailability == itemCount && !destroyWhenEmpty)
        {
            originalSprite.SetActive(false);
            emptySprite.SetActive(true);
        }
    }
}
