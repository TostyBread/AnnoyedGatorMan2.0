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
        }
    }

    void Update()
    {
        if (playerPickupSystem == null || ScreenToWorldPointMouse.Instance == null || playerInputManager == null)
            return;

        Collider2D target = playerPickupSystem.targetItem ?? playerPickupSystem.targetInteractable;

        if (target != null && IsMouseOver(target) && IsWithinRange(target))
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

    private bool IsWithinRange(Collider2D collider)
    {
        return Physics2D.OverlapCircle(playerPickupSystem.transform.position, playerPickupSystem.pickupRadius, 1 << collider.gameObject.layer) == collider;
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
