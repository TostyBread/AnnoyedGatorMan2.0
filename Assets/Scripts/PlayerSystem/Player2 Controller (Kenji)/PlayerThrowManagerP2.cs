using UnityEngine;
using System.Collections;

public class PlayerThrowManagerP2 : MonoBehaviour
{
    [Header("Throw Settings")]
    public float throwForceMultiplier = 2f;
    public float spinSpeed = 360f;
    public float quarterDistanceFactor = 0.5f;
    public float throwSpriteDuration = 0.5f;

    [Header("References")]
    public PlayerPickupSystemP2 playerPickupSystemP2;
    public HandSpriteManagerP2 handSpriteManagerP2;

    private bool isPreparingToThrow = false;
    private Vector2 storedThrowPosition;
    private IUsable usableFunction;

    public void StartPreparingThrow()
    {
        if (!playerPickupSystemP2.HasItemHeld) return;

        isPreparingToThrow = true;
        usableFunction = playerPickupSystemP2.GetUsableFunction();
        usableFunction?.DisableUsableFunction();
        Debug.Log("Preparing to throw...");
    }

    public void Throw()
    {
        if (!isPreparingToThrow) return;

        GameObject heldItem = playerPickupSystemP2.GetHeldItem();
        if (heldItem == null) return;

        if (heldItem.TryGetComponent(out FirearmController firearm))
        {
            firearm.ClearOwner();
        }

        playerPickupSystemP2.DropItem();
        handSpriteManagerP2?.ShowThrowSprite(throwSpriteDuration);

        storedThrowPosition = PlayerAimController.Instance.GetCursorPosition();

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