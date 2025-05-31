using UnityEngine;
using System.Collections.Generic;

public class TrashBag : MonoBehaviour
{
    public int trashBagDuration = 40;
    private int currentDuration = 0;

    private void Awake()
    {
        currentDuration = trashBagDuration;
    }

    public void SpillAndDestroy()
    {
        List<Transform> children = new();

        foreach (Transform child in transform)
            children.Add(child);

        foreach (Transform child in children)
        {
            child.gameObject.SetActive(true);
            child.SetParent(null);
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