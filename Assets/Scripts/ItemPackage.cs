using UnityEngine;

public class ItemPackage : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject itemPrefab;
    public int itemCount;
    public Transform spawnPosition;
    private int itemAvailability;

    private void Awake()
    {
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
        if(itemAvailability == itemCount)
        {
            this.gameObject.SetActive(false);
        }
    }
}
