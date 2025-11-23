using System.Collections.Generic;
using UnityEngine;

public class HeatBarManager : MonoBehaviour
{
    public GameObject heatBarPrefab; // The prefab for the heat bar
    public GameObject warningPrefab; // The prefab for the warning

    private Dictionary<GameObject, GameObject> spawnedBars = new Dictionary<GameObject, GameObject>();

    void Update()
    {
        // Find all cookable items
        GameObject[] cookableItems = GameObject.FindGameObjectsWithTag("FoodSmall");

        foreach (GameObject item in cookableItems)
        {

            // Skip if this item already has a bar
            if (spawnedBars.ContainsKey(item)) continue;

            SpawnHeatBarForItem(item);

            if (warningPrefab == null) return;
            GameObject warningInstance = Instantiate(warningPrefab, transform);

            Warning warning = warningInstance.GetComponent<Warning>();
            warning.target = item.transform;
        }

        RemoveHeatBar();
    }

    private void SpawnHeatBarForItem(GameObject item)
    {
        if (heatBarPrefab == null) return;

        // Instantiate heat bar as a child of HeatBarManager
        GameObject heatBarInstance = Instantiate(heatBarPrefab, transform);

        // Configure HeatBar script
        HeatBar heatBar = heatBarInstance.GetComponent<HeatBar>();
        heatBar.target = item.transform;             // The item it follows

        // Start empty
        if (heatBar.bar != null)
            heatBar.bar.fillAmount = 0f;

        // Track it to avoid duplicates
        spawnedBars.Add(item, heatBarInstance);
    }

    private void RemoveHeatBar()
    {
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
