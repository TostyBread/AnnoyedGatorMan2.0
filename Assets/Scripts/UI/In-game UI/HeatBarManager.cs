using System.Collections.Generic;
using UnityEngine;

public class HeatBarManager : MonoBehaviour
{
    public GameObject heatBarPrefab;   // Heat bar UI
    public GameObject warningPrefab;   // Warning UI

    private Dictionary<GameObject, GameObject> spawnedBars = new Dictionary<GameObject, GameObject>();
    private Dictionary<GameObject, GameObject> spawnedWarnings = new Dictionary<GameObject, GameObject>();

    void Update()
    {
        GameObject[] cookableItems = GameObject.FindGameObjectsWithTag("FoodSmall");

        foreach (GameObject item in cookableItems)
        {
            if (!spawnedBars.ContainsKey(item))
                SpawnHeatBarForItem(item);

            if (!spawnedWarnings.ContainsKey(item))
                SpawnWarningForItem(item);
        }

        CleanupDestroyedItems();
    }

    private void SpawnHeatBarForItem(GameObject item)
    {
        if (heatBarPrefab == null) return;

        GameObject heatBarInstance = Instantiate(heatBarPrefab, transform);

        HeatBar heatBar = heatBarInstance.GetComponent<HeatBar>();
        heatBar.target = item.transform;

        if (heatBar.bar != null)
            heatBar.bar.fillAmount = 0f;

        spawnedBars.Add(item, heatBarInstance);
    }

    private void SpawnWarningForItem(GameObject item)
    {
        if (warningPrefab == null) return;

        GameObject warningInstance = Instantiate(warningPrefab, transform);

        Warning warning = warningInstance.GetComponent<Warning>();
        warning.target = item.transform;

        spawnedWarnings.Add(item, warningInstance);
    }

    private void CleanupDestroyedItems()
    {
        List<GameObject> toRemove = new List<GameObject>();

        // Find items that got destroyed
        foreach (var kvp in spawnedBars)
        {
            if (kvp.Key == null)
                toRemove.Add(kvp.Key);
        }

        // Remove HeatBar + Warning
        foreach (GameObject key in toRemove)
        {
            // Remove heat bar
            if (spawnedBars.TryGetValue(key, out GameObject bar))
            {
                if (bar != null)
                    Destroy(bar);
            }
            spawnedBars.Remove(key);

            // Remove warning
            if (spawnedWarnings.TryGetValue(key, out GameObject warning))
            {
                if (warning != null)
                    Destroy(warning);
            }
            spawnedWarnings.Remove(key);
        }
    }
}
