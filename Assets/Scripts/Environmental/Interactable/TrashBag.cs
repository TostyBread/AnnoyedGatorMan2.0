using UnityEngine;

public class TrashBag : MonoBehaviour
{
    public int trashBagDuration = 40;
    private int currentDuration = 0;

    private void Awake()
    {
        currentDuration = trashBagDuration; // Assign health
    }
    public void SpillAndDestroy()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true); // Reactivate
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