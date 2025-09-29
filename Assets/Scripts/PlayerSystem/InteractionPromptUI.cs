using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionPromptUI : MonoBehaviour
{
    [Header("References")]
    public PlayerPickupSystem playerPickupSystem;
    public PlayerInputManager playerInputManager;

    [Header("Prompt UI")]
    public GameObject promptRoot;
    public Image radialFill;
    public TMP_Text keyBindText;

    [Header("Prompt Offset")]
    public Vector3 worldOffset = new Vector3(0f, 1f, 0f);

    private Transform targetTransform;
    private bool isPickupTarget;

    void Start()
    {
        if (playerInputManager != null && keyBindText != null)
        {
            keyBindText.text = playerInputManager.inputConfig.pickupKey.ToString();

            if (keyBindText.text == "RightControl") // change the long ass binding name into shorter version
            {
                keyBindText.text = "R CTRL";
            }
        }
    }

    void Update()
    {
        if (playerPickupSystem == null || ScreenToWorldPointMouse.Instance == null || playerInputManager == null)
            return;

        Collider2D target = playerPickupSystem.targetItem ?? playerPickupSystem.targetInteractable;

        if (target != null && IsMouseOver(target) && IsWithinRangeImproved(target))
        {
            isPickupTarget = playerPickupSystem.validTags.Contains(target.tag);
            targetTransform = target.transform;
            ShowPrompt(targetTransform.position + worldOffset);
            UpdateRadialProgress();
        }
        else
        {
            HidePrompt();
        }
    }

    private bool IsMouseOver(Collider2D collider)
    {
        Vector2 mouseWorldPos = ScreenToWorldPointMouse.Instance.GetMouseWorldPosition();
        return collider.OverlapPoint(mouseWorldPos);
    }

    private bool IsWithinRangeImproved(Collider2D targetCollider)
    {
        // Get all colliders within pickup radius, similar to how PlayerPickupSystem works
        Collider2D[] collidersInRange = Physics2D.OverlapCircleAll(playerPickupSystem.transform.position, playerPickupSystem.pickupRadius);
        
        // Check if our target collider is among the colliders in range and on the correct layer
        foreach (Collider2D collider in collidersInRange)
        {
            if (collider == targetCollider && collider.gameObject.layer == targetCollider.gameObject.layer)
            {
                return true;
            }
        }
        
        return false;
    }

    private void ShowPrompt(Vector3 worldPosition)
    {
        if (promptRoot != null)
        {
            promptRoot.SetActive(true);
            promptRoot.transform.position = worldPosition;
        }
    }

    private void HidePrompt()
    {
        if (promptRoot != null)
            promptRoot.SetActive(false);

        if (radialFill != null)
            radialFill.fillAmount = 0f;
    }

    private void UpdateRadialProgress()
    {
        if (radialFill == null || playerInputManager == null) return;

        if (isPickupTarget && playerInputManager.IsPickupKeyHeld())
        {
            radialFill.fillAmount = playerInputManager.PickupHoldProgress();
        }
        else
        {
            radialFill.fillAmount = 0f;
        }
    }
}
