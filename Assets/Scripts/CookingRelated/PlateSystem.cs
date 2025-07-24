using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlateRequirement
{
    public int itemID;
    public ItemDescriber.CookingState cookingState;
    public ItemDescriber.Condition condition;
    public Vector3 positionOffset;
    public int sortingOrder;
    public bool isFilled = false;
}

public class PlateSystem : MonoBehaviour
{
    public Transform rootPlateObject;
    public List<PlateRequirement> plateRequirements;
    public PlateMenuDisplay menuDisplay; // drag from scene
    public bool isReadyToServe { get; private set; } = false;
    private Dictionary<PlateRequirement, GameObject> placedItems = new Dictionary<PlateRequirement, GameObject>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out ItemDescriber itemDescriber)) return;

        PlateRequirement matchedRequirement = GetMatchingRequirement(itemDescriber);
        if (matchedRequirement != null && !matchedRequirement.isFilled)
        {
            AttachToPlate(other.gameObject, matchedRequirement);
            CheckIfAllRequirementsMet();
        }
    }

    private PlateRequirement GetMatchingRequirement(ItemDescriber item)
    {
        foreach (var req in plateRequirements)
        {
            if (!req.isFilled && req.itemID == item.itemID && req.cookingState == item.currentCookingState && req.condition == item.currentCondition)
            {
                return req;
            }
        }
        return null;
    }

    private void AttachToPlate(GameObject item, PlateRequirement requirement)
    {
        item.transform.SetParent(transform); // Attach to plate directly
        item.transform.localPosition = requirement.positionOffset;
        placedItems[requirement] = item;
        requirement.isFilled = true;

        int plateSortingOrder = 0;
        if (TryGetComponent(out SpriteRenderer plateRenderer))
            plateSortingOrder = plateRenderer.sortingOrder;

        UpdateSpriteSorting(item, plateSortingOrder + requirement.sortingOrder);
        FreezeItem(item);
        menuDisplay?.MarkAsFilled(requirement.itemID);
    }

    private void UpdateSpriteSorting(GameObject item, int sortingOrder)
    {
        if (item.TryGetComponent(out SpriteRenderer spriteRenderer))
        {
            spriteRenderer.sortingOrder = sortingOrder;
        }
    }

    private void FreezeItem(GameObject item)
    {
        if (item.TryGetComponent(out Collider2D collider)) collider.enabled = false;

        if (item.TryGetComponent(out Rigidbody2D rb))
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private void CheckIfAllRequirementsMet()
    {
        foreach (var req in plateRequirements)
        {
            if (!req.isFilled)
            {
                AudioManager.Instance.PlaySound("bell1", transform.position);
                return;
            }
        }

        AudioManager.Instance.PlaySound("TaskComplete", transform.position);
        isReadyToServe = true;
    }

    private bool isOwnerActive = true;
    private GameObject currentHolder = null;
    public bool IsHeld => currentHolder != null;

    public void SetOwnerActive(bool isActive)
    {
        isOwnerActive = isActive;
        if (!isActive && !IsHeld)
        {
            Destroy(gameObject);
        }
    }

    public void SetHolder(GameObject holder)
    {
        currentHolder = holder;
    }

    public void ClearHolder()
    {
        currentHolder = null;
        if (!isOwnerActive)
        {
            Destroy(gameObject);
        }
    }
}