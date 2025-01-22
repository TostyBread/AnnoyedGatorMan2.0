using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerPickupSystem : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float pickupRadius = 2f; // Range in which the player can pick up items
    public Transform handPosition; // Position where picked-up items will be held
    public float dropForce = 5f; // Force magnitude applied to dropped items

    public List<string> validTags = new List<string>(); // Valid tags for pickupable items

    private Collider2D targetItem = null; // The item currently being targeted
    private GameObject heldItem = null; // The currently held item
    private Coroutine pickupCoroutine = null; // Reference to the active pickup coroutine
    private bool isHoldingPickupKey = false; // Tracks if the player is holding the pickup key

    public HandSpriteManager handSpriteManager; // Reference to HandSpriteManager
    public CharacterFlip characterFlip; // Reference to CharacterFlip

    private MonoBehaviour usableItemController; // Reference to the usable item's controller (e.g., FirearmController)

    // Public property to check if the player is holding an item
    public bool HasItemHeld => heldItem != null;

    // Public property to get the tag of the held item
    public string HeldItemTag => heldItem != null ? heldItem.tag : null;

    // Public property to check if the held item has a usable function
    public bool HasUsableFunction => usableItemController != null;

    void Update()
    {
        HandleItemDetection();

        // Resume pickup if E key is held and conditions are met
        if (isHoldingPickupKey && targetItem != null && pickupCoroutine == null)
        {
            StartPickup();
        }
    }

    private void HandleItemDetection()
    {
        // Get the mouse position in world space
        Vector2 mouseWorldPos = ScreenToWorldPointMouse.Instance.GetMouseWorldPosition();

        // Find all colliders within the pickup radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, pickupRadius);

        // Reset the targetItem
        targetItem = null;

        // Check if the mouse is pointing at any valid item within range
        foreach (var collider in colliders)
        {
            if (IsPickupable(collider) && collider.OverlapPoint(mouseWorldPos))
            {
                targetItem = collider;
                break;
            }
        }
    }

    private bool IsPickupable(Collider2D collider)
    {
        // Check if the collider's tag matches any in the validTags list
        return validTags.Contains(collider.tag);
    }

    public void StartPickup()
    {
        if (targetItem != null && pickupCoroutine == null)
        {
            pickupCoroutine = StartCoroutine(PickupItemCoroutine());
        }
    }

    public void HoldPickup()
    {
        isHoldingPickupKey = true;

        // Resume pickup if conditions are met
        if (targetItem != null && pickupCoroutine == null)
        {
            StartPickup();
        }
    }

    public void CancelPickup()
    {
        isHoldingPickupKey = false;

        // Stop the active pickup process if it exists
        if (pickupCoroutine != null)
        {
            StopCoroutine(pickupCoroutine);
            pickupCoroutine = null;
        }
    }

    private IEnumerator PickupItemCoroutine()
    {
        float holdTime = 0.3f; // Time required to hold the key to pick up the item
        float elapsedTime = 0f;

        // Wait until the hold time is reached
        while (elapsedTime < holdTime)
        {
            // Cancel the pickup if the target becomes invalid or the key is released
            if (targetItem == null || !isHoldingPickupKey)
            {
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Successfully pick up the item
        if (targetItem != null)
        {
            // Drop the currently held item if necessary
            if (heldItem != null)
            {
                DropItem();
            }

            PickUpItem(targetItem.gameObject);
        }

        pickupCoroutine = null;
    }

    private void PickUpItem(GameObject item)
    {
        // Disable the item's collider and make it a child of the handPosition
        item.GetComponent<Collider2D>().enabled = false;

        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Parent the item to the handPosition
        item.transform.SetParent(handPosition);

        // Reset the item's local position and rotation
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        // Ensure the item's scale is set correctly
        Vector3 correctedScale = item.transform.localScale;
        correctedScale.x = 1; // Always set X-axis scale to 1
        item.transform.localScale = correctedScale;

        // Update the sprite's sorting order
        SpriteLayerManager layerManager = item.GetComponent<SpriteLayerManager>();
        if (layerManager != null)
        {
            layerManager.ChangeToHoldingOrder();
        }

        heldItem = item; // Update the reference to the held item

        // Check if the item has a usable function (e.g., FirearmController)
        usableItemController = heldItem.GetComponent<MonoBehaviour>();

        // Re-enable the usable function if it implements IUsable
        if (usableItemController != null && usableItemController is IUsable usableItem)
        {
            usableItem.EnableUsableFunction(); // Ensure usable function is re-enabled
        }

        // Notify the HandSpriteManager to update the player's hand sprite
        if (handSpriteManager != null)
        {
            handSpriteManager.UpdateHandSprite();
        }

        Debug.Log($"{item.name} has been picked up with corrected scale.");
    }

    public void DropItem()
    {
        if (heldItem != null)
        {
            // Calculate drop position based on character facing direction
            bool isFacingRight = characterFlip != null && characterFlip.IsFacingRight();
            float horizontalOffset = isFacingRight ? -0.2f : 0.5f;
            Vector3 dropPosition = handPosition.position + new Vector3(horizontalOffset, -0.5f, 0f);

            heldItem.transform.position = dropPosition;
            heldItem.transform.SetParent(null); // Detach the item

            // Revert the sprite's sorting order
            SpriteLayerManager layerManager = heldItem.GetComponent<SpriteLayerManager>();
            if (layerManager != null)
            {
                layerManager.RevertToOriginalOrder();
            }

            // Re-enable the item's collider
            Collider2D itemCollider = heldItem.GetComponent<Collider2D>();
            if (itemCollider != null)
            {
                itemCollider.enabled = true;
            }

            Rigidbody2D rb = heldItem.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.isKinematic = false;

                // Calculate direction to mouse position
                Vector2 mouseWorldPos = ScreenToWorldPointMouse.Instance.GetMouseWorldPosition();
                Vector2 direction = (mouseWorldPos - (Vector2)dropPosition).normalized;
                rb.AddForce(direction * dropForce, ForceMode2D.Impulse);
            }

            heldItem = null; // Clear the held item reference
            usableItemController = null; // Clear the usable function reference

            // Notify the HandSpriteManager to revert to the default sprite
            if (handSpriteManager != null)
            {
                handSpriteManager.UpdateHandSprite();
            }
        }
    }

    public GameObject GetHeldItem()
    {
        return heldItem;
    }

    public MonoBehaviour GetUsableFunction()
    {
        return usableItemController;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}