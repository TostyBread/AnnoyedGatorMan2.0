using System.Collections.Generic;
using UnityEngine;

public class HeatBarManager : MonoBehaviour
{
    public GameObject heatBarPrefab; // The prefab for the heat bar

    private Dictionary<GameObject, GameObject> spawnedBars = new Dictionary<GameObject, GameObject>();

    void Update()
    {
        // Find all cookable items
        GameObject[] cookableItems = GameObject.FindGameObjectsWithTag("FoodSmall");

        foreach (GameObject item in cookableItems)
        {
            // Skip if this item already has a bar
            if (spawnedBars.ContainsKey(item)) continue;

            // Instantiate heat bar as a child of HeatBarManager
            GameObject heatBarInstance = Instantiate(heatBarPrefab, transform);

            // Reset local transform
            heatBarInstance.transform.localPosition = Vector3.zero;
            heatBarInstance.transform.localRotation = Quaternion.identity;
            heatBarInstance.transform.localScale = Vector3.one;

            // Configure HeatBar script
            HeatBar heatBar = heatBarInstance.GetComponent<HeatBar>();
            heatBar.target = item.transform;             // The item it follows
            heatBar.offset = new Vector3(0, 1f, 0);     // Height above item

            // Start empty
            if (heatBar.bar != null)
                heatBar.bar.fillAmount = 0f;

            // Track it to avoid duplicates
            spawnedBars.Add(item, heatBarInstance);
        }

        // Cleanup destroyed items
        List<GameObject> toRemove = new List<GameObject>();
        foreach (var kvp in spawnedBars)
            if (kvp.Key == null) toRemove.Add(kvp.Key);

        foreach (var key in toRemove)
        {
            if (spawnedBars[key] != null)
                Destroy(spawnedBars[key]);
            spawnedBars.Remove(key);
        }
    }
}
