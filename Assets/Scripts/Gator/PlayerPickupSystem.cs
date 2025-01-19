using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerPickupSystem : MonoBehaviour
{
    public float pickupRadius = 2f; // Range in which the player can pick up items
    public Transform handPosition; // Position where picked-up items will be held
    public float dropForce = 5f; // Force magnitude applied to dropped items

    public List<string> validTags = new List<string> { "Weapon", "Food", "CookingTools" }; // Valid tags for pickupable items

    private Collider2D targetItem = null; // The item currently being targeted
    private GameObject heldItem = null; // The currently held item
    private Coroutine pickupCoroutine = null; // Reference to the active pickup coroutine
    public CharacterFlip characterFlip; // Reference to the CharacterFlip script


    void Update()
    {
        HandleItemDetection();
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
            if (IsPickupable(collider)) // Check if the item has a valid tag
            {
                // Check if the mouse is over this item
                if (collider.OverlapPoint(mouseWorldPos))
                {
                    targetItem = collider;
                    break;
                }
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
        // Start the pickup process if an item is targeted and no coroutine is active
        if (targetItem != null && pickupCoroutine == null)
        {
            pickupCoroutine = StartCoroutine(PickupItemCoroutine());
        }
    }

    public void HoldPickup()
    {
        // This method is called continuously while the key is held
        // You could add visual feedback for the hold progress here if needed
    }

    public void CancelPickup()
    {
        // Cancel the pickup process if the key is released
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

        // Wait for the hold time to elapse
        while (elapsedTime < holdTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Successfully picked up the item
        if (targetItem != null)
        {
            // Drop the currently held item (if any) before picking up the new item
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

        // If the item has a Rigidbody2D, freeze its movement
        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        item.transform.SetParent(handPosition);
        item.transform.localPosition = Vector3.zero; // Snap to hand position
        item.transform.localRotation = Quaternion.identity; // Reset rotation

        heldItem = item; // Update the heldItem reference
        Debug.Log("Picked up: " + item.name);
    }

    private void DropItem()
    {
        if (heldItem != null)
        {
            // Get the character's facing direction (right = -1, left = 1)
            bool isFacingRight = characterFlip != null && characterFlip.IsFacingRight();
            float horizontalOffset = isFacingRight ? -0.5f : 0.5f; // Adjust offset based on facing direction

            // Offset the item's starting position based on the hand's position and character's facing direction
            Vector3 dropPosition = handPosition.position + new Vector3(horizontalOffset, -0.5f, 0f); // Slightly below the hand
            heldItem.transform.position = dropPosition;

            // Detach the held item from the handPosition
            heldItem.transform.SetParent(null);

            // Enable the item's collider
            Collider2D itemCollider = heldItem.GetComponent<Collider2D>();
            if (itemCollider != null)
            {
                itemCollider.enabled = true;
            }

            // Calculate direction to mouse position
            Vector2 mouseWorldPos = ScreenToWorldPointMouse.Instance.GetMouseWorldPosition();
            Vector2 direction = (mouseWorldPos - (Vector2)dropPosition).normalized; // Normalized direction vector

            // If the item has a Rigidbody2D, apply a force in the direction of the mouse
            Rigidbody2D rb = heldItem.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddForce(direction * dropForce, ForceMode2D.Impulse);
            }

            Debug.Log("Dropped: " + heldItem.name + " towards mouse.");
            heldItem = null; // Clear the heldItem reference
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the pickup radius in the scene view for debugging
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}