using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlateRequirement
{
    public string itemName;
    public ItemDescriber.CookingState cookingState;
    public ItemDescriber.Condition condition;
    public Vector3 positionOffset; // Offset relative to plate
    public int sortingOrder; // Sorting order for layering
    public bool isFilled = false; // Tracks if this requirement is already satisfied
}

public class PlateSystem : MonoBehaviour
{
    public Transform plateParent; // Parent to hold items
    public List<PlateRequirement> plateRequirements; // Structured list for validation
    private Dictionary<PlateRequirement, GameObject> placedItems = new Dictionary<PlateRequirement, GameObject>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        ItemDescriber itemDescriber = other.GetComponent<ItemDescriber>();
        if (itemDescriber != null)
        {
            PlateRequirement matchedRequirement = GetMatchingRequirement(itemDescriber);
            if (matchedRequirement != null && !matchedRequirement.isFilled)
            {
                AttachToPlate(other.gameObject, matchedRequirement);
            }
        }
    }

    private PlateRequirement GetMatchingRequirement(ItemDescriber item)
    {
        foreach (PlateRequirement req in plateRequirements)
        {
            if (!req.isFilled && req.itemName == item.itemName &&
                req.cookingState == item.currentCookingState &&
                req.condition == item.currentCondition)
            {
                return req; // Return the first available matching requirement
            }
        }
        return null;
    }

    private void AttachToPlate(GameObject item, PlateRequirement requirement)
    {
        item.transform.SetParent(plateParent);
        item.transform.localPosition = requirement.positionOffset; // Ensures correct offset relative to plate
        placedItems[requirement] = item;
        requirement.isFilled = true;

        UpdateSpriteSorting(item, requirement.sortingOrder);
        FreezeItem(item);
    }

    private void UpdateSpriteSorting(GameObject item, int sortingOrder)
    {
        SpriteRenderer spriteRenderer = item.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = sortingOrder;
        }
    }

    private void FreezeItem(GameObject item)
    {
        Collider2D collider = item.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f; // Disable gravity
            rb.bodyType = RigidbodyType2D.Kinematic; // Allow movement with parent
        }
    }

}