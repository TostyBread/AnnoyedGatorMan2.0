using UnityEngine;

public class SpriteLayerManager : MonoBehaviour
{
    [Header("Layer Order Settings")]
    public int holdingOrder = 2; // Order in Layer when the item is held
    private int originalOrder; // Stores the original Order in Layer

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // Get the SpriteRenderer from the child GameObject
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalOrder = spriteRenderer.sortingOrder; // Cache the original sorting order
        }
        else
        {
            Debug.LogError($"SpriteRenderer is missing on {gameObject.name} or its children.");
        }
    }

    // Call this when the item is picked up
    public void ChangeToHoldingOrder()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = holdingOrder; // Set the "holding" sorting order
        }
    }

    // Call this when the item is dropped
    public void RevertToOriginalOrder()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = originalOrder; // Revert to the original sorting order
        }
    }
}