using UnityEngine;
using System.Collections;

public class PlayerThrowManager : MonoBehaviour
{
    [Header("Throw Settings")]
    public float throwForceMultiplier = 2f;
    public float spinSpeed = 360f;
    public float throwSpriteDuration = 0.5f;


    [Header("If P1, make sure p2PickSystem is null \nIf P2, make sure p2PickupSystem is null")]
    public bool P1FalseP2True;
    public Transform P2ThrowDirection;

    [Header("References")]
    public PlayerPickupSystem playerPickupSystem;
    public P2PickupSystem p2PickSystem;
    public HandSpriteManager handSpriteManager;

    private Vector2 storedThrowPosition;
    public bool doorCauseThrow; // REMOVE doorCauseThrow, the door mechanic will not be implemented

    public void Throw()
    {
        GameObject heldItem = P1FalseP2True
            ? p2PickSystem.GetHeldItem()
            : playerPickupSystem.GetHeldItem();

        if (heldItem == null) return;

        // Detach firearm ownership if applicable
        if (heldItem.TryGetComponent(out FirearmController firearm))
        {
            firearm.ClearOwner();
        }

        if (P1FalseP2True) p2PickSystem.DropItem();
        else playerPickupSystem.DropItem();

        handSpriteManager?.ShowThrowSprite(throwSpriteDuration);

        // Determine throw direction
        if (doorCauseThrow) // REMOVE doorCauseThrow, the door mechanic will not be implemented
        {
            storedThrowPosition = transform.position;
        }
        else
        {
            if (P1FalseP2True)
            {
                if (P2ThrowDirection == null)
                {
                    Debug.LogError("P2ThrowDirection is not assigned.");
                    return;
                }
                storedThrowPosition = P2ThrowDirection.position;
            }
            else
            {
                storedThrowPosition = ScreenToWorldPointMouse.Instance.GetMouseWorldPosition();
            }
        }

        float distance = Vector2.Distance(transform.position, storedThrowPosition);

        // Configurable speeds
        float minDuration = 0.15f;
        float maxDuration = 1.5f;
        float maxThrowSpeed = 2f; // units per second cap

        // Base travel time from throw force
        float travelTime = distance / throwForceMultiplier;

        // Apply speed cap
        float cappedTime = distance / maxThrowSpeed;
        travelTime = Mathf.Max(travelTime, cappedTime);

        // Clamp into min/max bounds
        travelTime = Mathf.Clamp(travelTime, minDuration, maxDuration);


        if (heldItem.TryGetComponent(out Rigidbody2D rb))
        {
            rb.isKinematic = true; // Disable physics while animating
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        Collider2D itemCollider = heldItem.GetComponent<Collider2D>();
        if (itemCollider) itemCollider.enabled = false;

        StartCoroutine(SimulatedThrow(heldItem, storedThrowPosition, travelTime, distance, itemCollider));

        AudioManager.Instance.PlaySound("slash1", transform.position);
    }

    private IEnumerator SimulatedThrow(GameObject item, Vector2 targetPos, float duration, float distance, Collider2D itemCollider)
    {
        if (item == null) yield break;

        Vector2 startPos = item.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Ease out cubic (fast start, smooth slow end)
            float easedT = 1 - Mathf.Pow(1 - t, 3);

            item.transform.position = Vector2.Lerp(startPos, targetPos, easedT);
            item.transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime * (targetPos.x > startPos.x ? -1 : 1));

            yield return null;
        }

        // Snap to final target
        item.transform.position = targetPos;

        if (itemCollider != null) itemCollider.enabled = true;

        if (item.TryGetComponent(out Rigidbody2D rb))
        {
            rb.isKinematic = false; // Re-enable physics
        }
    }
}