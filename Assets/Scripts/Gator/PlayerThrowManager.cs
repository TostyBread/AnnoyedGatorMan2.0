using UnityEngine;
using System.Collections;

public class PlayerThrowManager : MonoBehaviour
{
    [Header("Throw Settings")]
    public float throwForceMultiplier = 2f; // Multiplier to scale throw force based on distance
    public float spinSpeed = 360f; // Spin speed of the item during flight
    public float quarterDistanceFactor = 0.5f; // When to re-enable collider (50% of trajectory)
    public float throwSpriteDuration = 0.5f; // Duration to show the throw sprite

    [Header("If P1, make sure p2PickSystem is null \nIf P2, make sure playerPickupSystem is null")]
    public bool P1FalseP2True;
    public Transform P2ThrowDirection;

    [Header("References")]
    public PlayerPickupSystem playerPickupSystem; // Reference to PlayerPickupSystem
    public P2PickSystem p2PickSystem; // Reference to P2PickSystem
    public HandSpriteManager handSpriteManager; // Reference to HandSpriteManager for sprite toggling

    private bool isPreparingToThrow = false; // Tracks if the player is preparing to throw
    private Vector2 storedThrowPosition; // Stores the last mouse click position
    private IUsable usableFunction; // Cache of the usable function (if any)


    public void StartPreparingThrow()
    {
        bool hasItem = P1FalseP2True ? p2PickSystem.HasItemHeld : playerPickupSystem.HasItemHeld;
        if (!hasItem) return;

        isPreparingToThrow = true;

        usableFunction = P1FalseP2True
            ? p2PickSystem.GetUsableFunction()
            : playerPickupSystem.GetUsableFunction();

        usableFunction?.DisableUsableFunction();
        Debug.Log("Preparing to throw...");
    }

    public void Throw()
    {
        if (!isPreparingToThrow) return;

        GameObject heldItem = P1FalseP2True
            ? p2PickSystem.GetHeldItem()
            : playerPickupSystem.GetHeldItem();

        if (heldItem == null) return;

        if (P1FalseP2True)
        {
            p2PickSystem.DropItem(false);
        }
        else
        {
            playerPickupSystem.DropItem();
        }

        handSpriteManager?.ShowThrowSprite(throwSpriteDuration);

        if (!P1FalseP2True)
        {
            storedThrowPosition = ScreenToWorldPointMouse.Instance.GetMouseWorldPosition(); //this affect the direction of where P1 throw object
        }
        else if (P1FalseP2True)
        {
            if (P2ThrowDirection == null)
            {
                Debug.LogError("P2ThrowDirection missing");
                return;
            }

            storedThrowPosition = P2ThrowDirection.position;
        }

        float distance = Vector2.Distance(transform.position, storedThrowPosition);
        float adjustedThrowForce = distance * throwForceMultiplier;

        if (!heldItem.TryGetComponent(out Rigidbody2D rb))
        {
            rb = heldItem.AddComponent<Rigidbody2D>();
        }

        if (heldItem.TryGetComponent(out Collider2D itemCollider))
        {
            itemCollider.enabled = false;
        }

        Vector2 throwDirection = (storedThrowPosition - (Vector2)transform.position).normalized;
        rb.isKinematic = false;
        rb.velocity = throwDirection * adjustedThrowForce;
        rb.angularVelocity = spinSpeed * (throwDirection.x > 0 ? -1 : 1);

        StartCoroutine(EnableColliderDuringTrajectory(heldItem, heldItem.GetComponent<Collider2D>(), distance));
        isPreparingToThrow = false;

        AudioManager.Instance.PlaySound("slash1", 1.0f, transform.position);
    }

    public void CancelThrow()
    {
        if (!isPreparingToThrow) return;

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