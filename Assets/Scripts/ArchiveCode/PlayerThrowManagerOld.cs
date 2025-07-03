using UnityEngine;
using System.Collections;

public class PlayerThrowManagerOld : MonoBehaviour
{
    [Header("Throw Settings")]
    public float throwForceMultiplier = 2f;
    public float spinSpeed = 360f;
    public float quarterDistanceFactor = 0.5f;
    public float throwSpriteDuration = 0.5f;

    [Header("References")]
    public PlayerPickupSystem playerPickupSystem;
    public HandSpriteManager handSpriteManager;

    private bool isPreparingToThrow = false;
    private Vector2 storedThrowPosition;
    private IUsable usableFunction;

    public void StartPreparingThrow()
    {
        if (isPreparingToThrow || !playerPickupSystem.HasItemHeld) return;

        isPreparingToThrow = true;
        usableFunction = playerPickupSystem.GetUsableFunction();
        usableFunction?.DisableUsableFunction();
    }

    public void Throw()
    {
        if (!isPreparingToThrow) return;

        GameObject heldItem = playerPickupSystem.GetHeldItem();
        if (heldItem == null) return;

        if (heldItem.TryGetComponent(out FirearmController firearm))
        {
            firearm.ClearOwner();
        }

        playerPickupSystem.DropItem();
        handSpriteManager?.ShowThrowSprite(throwSpriteDuration);

        storedThrowPosition = ScreenToWorldPointMouse.Instance.GetMouseWorldPosition();

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

        AudioManager.Instance.PlaySound("slash1", transform.position);
    }

    public void CancelThrow()
    {
        if (!isPreparingToThrow) return;

        usableFunction?.EnableUsableFunction();
        isPreparingToThrow = false;
        //Debug.Log("Throw preparation canceled.");
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