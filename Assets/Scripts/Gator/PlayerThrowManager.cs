using UnityEngine;
using System.Collections;

public class PlayerThrowManager : MonoBehaviour
{
    [Header("Throw Settings")]
    public float throwForceMultiplier = 2f; // Multiplier to scale throw force based on distance
    public float spinSpeed = 360f; // Spin speed of the item during flight
    public float quarterDistanceFactor = 0.25f; // When to re-enable collider (25% of trajectory)
    public float throwSpriteDuration = 0.5f; // Duration to show the throw sprite

    [Header("References")]
    public PlayerPickupSystem playerPickupSystem; // Reference to PlayerPickupSystem
    public HandSpriteManager handSpriteManager; // Reference to HandSpriteManager for sprite toggling

    private bool isPreparingToThrow = false; // Tracks if the player is preparing to throw
    private Vector2 storedThrowPosition; // Stores the last mouse click position
    private MonoBehaviour usableFunction; // Cache of the usable function (if any)

    public void StartPreparingThrow()
    {
        if (playerPickupSystem == null || !playerPickupSystem.HasItemHeld) return;

        isPreparingToThrow = true;

        // Disable the usable function of the held item, if it has one
        usableFunction = playerPickupSystem.GetUsableFunction();
        if (usableFunction != null && usableFunction is IUsable usableItem)
        {
            usableItem.DisableUsableFunction();
        }

        // Optional: Add visual feedback for the throw arc here
        Debug.Log("Preparing to throw...");
    }

    public void Throw()
    {
        if (!isPreparingToThrow || playerPickupSystem == null || !playerPickupSystem.HasItemHeld) return;

        GameObject heldItem = playerPickupSystem.GetHeldItem();
        if (heldItem == null) return;

        // Detach the item from the player's hand
        playerPickupSystem.DropItem();

        // Notify HandSpriteManager to show the throw sprite
        if (handSpriteManager != null)
        {
            handSpriteManager.ShowThrowSprite(throwSpriteDuration);
        }

        // Store the mouse position at the moment of the throw
        storedThrowPosition = ScreenToWorldPointMouse.Instance.GetMouseWorldPosition();

        // Calculate the distance to the target and scale the throw force
        float distance = Vector2.Distance(transform.position, storedThrowPosition);
        float adjustedThrowForce = distance * throwForceMultiplier;

        // Add a Rigidbody2D to the item if not already present
        Rigidbody2D rb = heldItem.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = heldItem.AddComponent<Rigidbody2D>();
        }

        // Disable the item's collider during flight
        Collider2D itemCollider = heldItem.GetComponent<Collider2D>();
        if (itemCollider != null)
        {
            itemCollider.enabled = false; // Disable collider during flight
        }

        // Calculate the throw direction and apply force
        Vector2 throwDirection = (storedThrowPosition - (Vector2)transform.position).normalized;
        rb.isKinematic = false;
        rb.velocity = throwDirection * adjustedThrowForce;

        // Add angular velocity for spinning effect
        rb.angularVelocity = spinSpeed * (throwDirection.x > 0 ? -1 : 1); // Spin clockwise or counterclockwise based on direction

        // Start coroutine to handle re-enabling collider partway through trajectory
        StartCoroutine(EnableColliderDuringTrajectory(heldItem, rb, itemCollider, distance));
        isPreparingToThrow = false; // Reset throw state
    }

    public void CancelThrow()
    {
        if (!isPreparingToThrow || playerPickupSystem == null) return;

        // Re-enable the usable function of the held item, if it has one
        if (usableFunction != null && usableFunction is IUsable usableItem)
        {
            usableItem.EnableUsableFunction();
        }

        isPreparingToThrow = false;
        Debug.Log("Throw preparation canceled.");
    }

    private IEnumerator EnableColliderDuringTrajectory(GameObject item, Rigidbody2D rb, Collider2D itemCollider, float totalDistance)
    {
        if (itemCollider == null) yield break;

        // Wait until the item has traveled 25% of the total distance to the target
        float quarterDistance = totalDistance * quarterDistanceFactor;
        while (Vector2.Distance(item.transform.position, storedThrowPosition) > totalDistance - quarterDistance)
        {
            yield return null; // Wait for the next frame
        }

        // Re-enable the collider
        itemCollider.enabled = true;
    }
}