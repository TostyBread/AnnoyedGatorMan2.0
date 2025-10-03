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

    public int plateScore = 1;

    // Optimization: Cache components and requirements lookup
    private SpriteRenderer cachedPlateRenderer;
    private int cachedPlateSortingOrder;
    private Dictionary<(int itemID, ItemDescriber.CookingState cookingState, ItemDescriber.Condition condition), List<PlateRequirement>> requirementLookup;
    private int unfilledRequirementCount;

    private void Awake()
    {
        // Cache the plate's SpriteRenderer once
        cachedPlateRenderer = GetComponent<SpriteRenderer>();
        cachedPlateSortingOrder = cachedPlateRenderer != null ? cachedPlateRenderer.sortingOrder : 0;

        // Build lookup dictionary for O(1) requirement matching
        BuildRequirementLookup();
    }

    private void BuildRequirementLookup()
    {
        requirementLookup = new Dictionary<(int, ItemDescriber.CookingState, ItemDescriber.Condition), List<PlateRequirement>>();
        unfilledRequirementCount = plateRequirements.Count;

        foreach (var req in plateRequirements)
        {
            var key = (req.itemID, req.cookingState, req.condition);

            // Support multiple requirements of the same type
            if (!requirementLookup.ContainsKey(key))
            {
                requirementLookup[key] = new List<PlateRequirement>();
            }
            requirementLookup[key].Add(req);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Early exit if plate is already complete
        if (isReadyToServe) return;

        if (!other.TryGetComponent(out ItemDescriber itemDescriber)) return;

        // Additional safety check: ensure item is not already attached to any plate
        if (itemDescriber.isAttachedToPlate) return;

        // O(1) lookup to get list of matching requirements
        var key = (itemDescriber.itemID, itemDescriber.currentCookingState, itemDescriber.currentCondition);
        if (requirementLookup.TryGetValue(key, out List<PlateRequirement> matchingRequirements))
        {
            // Find the first unfilled requirement of this type
            PlateRequirement availableRequirement = null;
            foreach (var req in matchingRequirements)
            {
                if (!req.isFilled)
                {
                    availableRequirement = req;
                    break;
                }
            }

            if (availableRequirement != null)
            {
                // Try to claim the item atomically - this prevents race conditions
                if (itemDescriber.TryAttachToPlate(this))
                {
                    AttachToPlate(other.gameObject, availableRequirement);

                    // Decrement counter and check completion
                    unfilledRequirementCount--;
                    if (unfilledRequirementCount == 0)
                    {
                        CompleteAllRequirements();
                    }
                    else
                    {
                        AudioManager.Instance.PlaySound("bell1", transform.position);
                    }
                }
            }
        }
    }

    private void AttachToPlate(GameObject item, PlateRequirement requirement)
    {
        item.transform.SetParent(transform); // Attach to plate directly
        item.transform.localPosition = requirement.positionOffset;
        placedItems[requirement] = item;
        requirement.isFilled = true;

        // Use cached sorting order instead of repeated component lookup
        UpdateSpriteSorting(item, cachedPlateSortingOrder + requirement.sortingOrder);
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
        // Cache component lookups
        var itemCollider = item.GetComponent<Collider2D>();
        if (itemCollider) itemCollider.enabled = false;

        var rb = item.GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    // Optimized completion check - no more foreach loop
    private void CompleteAllRequirements()
    {
        AudioManager.Instance.PlaySound("TaskComplete", transform.position);
        isReadyToServe = true;
    }

    // Cleanup method for detaching items (optimized version)
    private void DetachAllItems()
    {
        foreach (var placedItem in placedItems.Values)
        {
            if (placedItem != null && placedItem.TryGetComponent(out ItemDescriber itemDescriber))
            {
                itemDescriber.DetachFromPlate();
            }
        }
        placedItems.Clear();
    }

    private bool isOwnerActive = true;
    private GameObject currentHolder = null;
    public GameObject lastHolder = null;
    public bool IsHeld => currentHolder != null;

    // Always returns the remembered last holder
    public GameObject LastHolder => lastHolder;

    public void SetOwnerActive(bool isActive)
    {
        isOwnerActive = isActive;
        if (!isActive && !IsHeld)
        {
            DetachAllItems();
            Destroy(gameObject);
        }
    }

    public void SetHolder(GameObject holder)
    {
        currentHolder = holder;
        lastHolder = holder;
    }

    public void ClearHolder()
    {
        currentHolder = null;
        if (!isOwnerActive)
        {
            DetachAllItems();
            Destroy(gameObject);
        }
    }

    // For player throw managers to check if the owner is still active
    public bool IsOwnerActive()
    {
        return isOwnerActive;
    }
}