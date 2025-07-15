using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [Header("References")]
    public PlayerPickupSystem playerPickupSystem;

    [Header("Prompt Images (World Space)")]
    public GameObject pickupPromptImage;
    public GameObject interactPromptImage;

    [Header("Prompt Offset")]
    public Vector3 worldOffset = new Vector3(0f, 1f, 0f); // Offset above the object

    void Update()
    {
        if (playerPickupSystem == null || ScreenToWorldPointMouse.Instance == null)
            return;

        Collider2D target = playerPickupSystem.targetItem ?? playerPickupSystem.targetInteractable;

        if (target != null && IsMouseOver(target) && IsWithinRange(target))
        {
            bool isPickup = playerPickupSystem.validTags.Contains(target.tag);
            ShowPrompt(isPickup, target.transform.position + worldOffset);
        }
        else
        {
            HidePrompts();
        }
    }

    private bool IsMouseOver(Collider2D collider)
    {
        Vector2 mouseWorldPos = ScreenToWorldPointMouse.Instance.GetMouseWorldPosition();
        return collider.OverlapPoint(mouseWorldPos);
    }

    private bool IsWithinRange(Collider2D collider)
    {
        return Physics2D.OverlapCircle(playerPickupSystem.transform.position, playerPickupSystem.pickupRadius, 1 << collider.gameObject.layer) == collider;
    }

    private void ShowPrompt(bool showPickup, Vector3 worldPosition)
    {
        if (pickupPromptImage != null)
        {
            pickupPromptImage.SetActive(showPickup);
            if (showPickup)
                pickupPromptImage.transform.position = worldPosition;
        }

        if (interactPromptImage != null)
        {
            interactPromptImage.SetActive(!showPickup);
            if (!showPickup)
                interactPromptImage.transform.position = worldPosition;
        }
    }

    private void HidePrompts()
    {
        if (pickupPromptImage != null) pickupPromptImage.SetActive(false);
        if (interactPromptImage != null) interactPromptImage.SetActive(false);
    }
}