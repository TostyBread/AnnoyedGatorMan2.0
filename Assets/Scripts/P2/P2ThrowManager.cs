using UnityEngine;
using System.Collections;

public class P2ThrowManager : MonoBehaviour
{
    [Header("Throw Settings")]
    public float throwForceMultiplier = 2f; // Multiplier to scale throw force based on distance
    public float spinSpeed = 360f; // Spin speed of the item during flight
    public float quarterDistanceFactor = 0.5f; // When to re-enable collider (50% of trajectory)
    public float throwSpriteDuration = 0.5f; // Duration to show the throw sprite

    [Header("References")]
    public P2PickSystem playerPickupSystem; // Reference to PlayerPickupSystem
    public HandSpriteManager handSpriteManager; // Reference to HandSpriteManager for sprite toggling

    private bool isPreparingToThrow = false; // Tracks if the player is preparing to throw
    private Vector2 storedThrowPosition; // Stores the last mouse click position
    private IUsable usableFunction; // Cache of the usable function (if any)

    public void StartPreparingThrow()
    {
        if (playerPickupSystem == null || !playerPickupSystem.HasItemHeld) return;

        isPreparingToThrow = true;
        usableFunction = playerPickupSystem.GetUsableFunction() as IUsable;
        usableFunction?.DisableUsableFunction();

        Debug.Log("Preparing to throw...");
    }

    public void Throw()
    {
        if (!isPreparingToThrow || playerPickupSystem == null || !playerPickupSystem.HasItemHeld) return;

        GameObject heldItem = playerPickupSystem.GetHeldItem();
        if (heldItem == null) return;

        playerPickupSystem.DropItem();
        handSpriteManager?.ShowThrowSprite(throwSpriteDuration);

        storedThrowPosition = ScreenToWorldPointMouse.Instance.GetMouseWorldPosition();
        float distance = Vector2.Distance(transform.position, storedThrowPosition);
        float adjustedThrowForce = distance * throwForceMultiplier;

        Rigidbody2D rb = heldItem.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = heldItem.AddComponent<Rigidbody2D>();  // Add Rigidbody2D if it doesn't exist
        }

        rb.velocity = Vector2.zero; // Reset velocity
        rb.angularVelocity = 0f;    // Reset angular velocity

        if (heldItem.TryGetComponent(out Collider2D itemCollider))
        {
            itemCollider.enabled = false; // Disable collider during flight
        }

        Vector2 throwDirection = (storedThrowPosition - (Vector2)transform.position).normalized;
        rb.isKinematic = false;  // Enable physics interaction
        rb.velocity = throwDirection * adjustedThrowForce;
        rb.angularVelocity = spinSpeed * (throwDirection.x > 0 ? -1 : 1);

        StartCoroutine(EnableColliderDuringTrajectory(heldItem, itemCollider, distance));
        isPreparingToThrow = false;

        AudioManager.Instance.PlaySound("slash1", 1.0f, transform.position);
    }

    public void CancelThrow()
    {
        if (!isPreparingToThrow || playerPickupSystem == null) return;

        usableFunction?.EnableUsableFunction();
        isPreparingToThrow = false;
        Debug.Log("Throw preparation canceled.");
    }

    private IEnumerator EnableColliderDuringTrajectory(GameObject item, Collider2D itemCollider, float totalDistance)
    {
        if (itemCollider == null) yield break;

        float enableDelay = Mathf.Clamp(totalDistance * 0.05f, 0.1f, 0.3f);
        yield return new WaitForSeconds(enableDelay);

        if (itemCollider != null)
        {
            itemCollider.enabled = true;
        }
    }
}