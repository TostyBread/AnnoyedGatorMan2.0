using UnityEngine;
using System.Collections.Generic;

public class TrashBag : MonoBehaviour
{
    public int trashBagDuration = 40;
    private int currentDuration = 0;

    // Instance-based pre-allocated list
    private readonly List<Transform> childList = new List<Transform>();

    private void Awake()
    {
        currentDuration = trashBagDuration;
    }

    public void SpillAndDestroy()
    {
        childList.Clear();

        // Collect children
        foreach (Transform child in transform)
            childList.Add(child);

        // Process children
        for (int i = 0; i < childList.Count; i++)
        {
            Transform child = childList[i];
            if (child != null)
            {
                child.gameObject.SetActive(true);
                child.SetParent(null);
            }
        }

        Destroy(gameObject);
    }

    public void TryDamaging(int damage)
    {
        currentDuration -= damage;

        if (currentDuration <= 0)
        {
            SpillAndDestroy();
        }
    }
}