using UnityEngine;
using System.Collections;

public class MeleeSwingP2 : MonoBehaviour
{
    [Header("Swing Settings")]
    public float swingAngle = 90f;
    public float swingDuration = 0.2f;
    public float swingCooldown = 0.3f;

    [Header("Hand Setup")]
    public Transform handTransform;
    public Transform pivotPoint;
    public PlayerPickupSystemP2 pickupSystemP2;
    public HandControllerP2 handControllerP2;
    public CharacterFlipP2 characterFlipP2;

    private bool isSwinging = false;
    private bool isCooldown = false;

    public void Use()
    {
        if (isSwinging || isCooldown || pickupSystemP2 == null || !pickupSystemP2.HasItemHeld || Input.GetKey(KeyCode.Joystick1Button4))
            return;

        GameObject heldItem = pickupSystemP2.GetHeldItem();
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

        GameObject heldItem = pickupSystemP2.GetHeldItem();
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

        Vector3 vMousePos = PlayerAimController.Instance.GetCursorPosition();
        Vector3 direction = (vMousePos - pivotPoint.position).normalized;

        float startAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bool isFacingRight = characterFlipP2 != null && characterFlipP2.IsFacingRight();
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

        if (handControllerP2 != null)
        {
            handControllerP2.enabled = true;
        }

        heldCollider.enabled = false;
        isSwinging = false;
        yield return new WaitForSeconds(swingCooldown);
        isCooldown = false;
    }
}