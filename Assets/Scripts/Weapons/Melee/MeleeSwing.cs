using UnityEngine;
using System.Collections;

public class MeleeSwing : MonoBehaviour
{
    [Header("Swing Settings")]
    public float swingAngle = 90f;
    public float swingDuration = 0.2f;
    public float swingCooldown = 0.3f;

    [Header("Hand Setup")]
    public Transform handTransform;
    public Transform pivotPoint;
    public PlayerPickupSystem pickupSystem;
    public HandController handController;
    public CharacterFlip characterFlip;

    private bool isSwinging = false;
    private bool isCooldown = false;

    public void Use()
    {
        if (isSwinging || isCooldown || pickupSystem == null || !pickupSystem.HasItemHeld || Input.GetMouseButton(1))
            return;

        GameObject heldItem = pickupSystem.GetHeldItem();
        if (heldItem == null)
            return;

        IUsable usable = heldItem.GetComponent<IUsable>();
        if (usable != null && usable.IsInUsableMode())
            return;

        StartCoroutine(SwingCoroutine());
    }

    private IEnumerator SwingCoroutine()
    {
        isSwinging = true;
        isCooldown = true;

        GameObject heldItem = pickupSystem.GetHeldItem();
        if (heldItem == null)
        {
            isSwinging = false;
            isCooldown = false;
            yield break;
        }

        Collider2D heldCollider = heldItem.GetComponent<Collider2D>();
        if (heldCollider != null)
        {
            heldCollider.enabled = true;
        }

        Vector3 mousePos = ScreenToWorldPointMouse.Instance.GetMouseWorldPosition();
        Vector3 direction = (mousePos - pivotPoint.position).normalized;

        float startAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bool isFacingRight = characterFlip != null && characterFlip.IsFacingRight();
        if (!isFacingRight)
        {
            startAngle = 180f + Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }

        float halfSwing = swingAngle / 2f;
        float fromAngle = startAngle - halfSwing;
        float toAngle = startAngle + halfSwing;

        float elapsed = 0f;
        while (elapsed < swingDuration)
        {
            float t = elapsed / swingDuration;
            float angle = Mathf.Lerp(fromAngle, toAngle, t);
            pivotPoint.rotation = Quaternion.Euler(0f, 0f, angle);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (handController != null)
        {
            handController.enabled = true;
        }

        heldCollider.enabled = false;
        isSwinging = false;
        yield return new WaitForSeconds(swingCooldown);
        isCooldown = false;
    }
}