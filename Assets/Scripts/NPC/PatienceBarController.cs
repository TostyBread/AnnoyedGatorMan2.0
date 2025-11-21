using UnityEngine;
using UnityEngine.UI;

public class PatienceBarController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fillImage;
    [SerializeField] private Transform targetTransform;

    [Header("Color Thresholds")]
    [SerializeField] private float midThreshold = 0.5f;
    [SerializeField] private float lowThreshold = 0.2f;

    [SerializeField] private Color normalColor = Color.green;
    [SerializeField] private Color midColor = Color.yellow;
    [SerializeField] private Color lowColor = Color.red;

    [Header("Flash")]
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashInterval = 0.1f;

    [Header("Up-Down Movement")]
    [SerializeField] private float moveIntervalMin = 1f; // Delay between jumps
    [SerializeField] private float moveIntervalMax = 3f; // Delay between jumps
    [SerializeField] private float moveDistance = 0.2f;
    [SerializeField] private float moveSpeed = 2f;

    private Color originalColor;
    private float currentFlashInterval;

    private bool isFlashing = false;
    private float flashDuration;
    private float currentMoveTime = 0f;
    private Vector3 jumpStartPosition;
    private bool isJumping = false;
    private float jumpTimer = 0f;
    private PatienceLevel currentPatience = PatienceLevel.Normal;

    private enum PatienceLevel
    {
        Normal,
        Mid,
        Low
    }

    private void Start()
    {
        currentFlashInterval = flashInterval;
        if (targetTransform != null)
        {
            jumpStartPosition = targetTransform.position;
        }
    }

    private void FixedUpdate()
    {
        if (targetTransform == null)
            return;

        if (currentPatience == PatienceLevel.Mid)
        {
            HandleJumpTiming(moveIntervalMax);
        }
        else if (currentPatience == PatienceLevel.Low)
        {
            HandleJumpTiming(moveIntervalMin);
        }

        if (isJumping)
        {
            PerformJump();
        }
    }

    private void HandleJumpTiming(float interval)
    {
        currentMoveTime -= Time.deltaTime;

        if (currentMoveTime <= 0 && !isJumping)
        {
            isJumping = true;
            jumpStartPosition = targetTransform.position;
            jumpTimer = 0f;
            currentMoveTime = interval;
        }
    }

    private void PerformJump()
    {
        jumpTimer += Time.deltaTime;
        float jumpDuration = 1f / moveSpeed;

        if (jumpTimer < jumpDuration)
        {
            float t = jumpTimer / jumpDuration;
            // Parabolic jump: up then down
            float height = Mathf.Sin(t * Mathf.PI) * moveDistance;
            targetTransform.position = jumpStartPosition + Vector3.up * height;
        }
        else
        {
            targetTransform.position = jumpStartPosition;
            isJumping = false;
        }
    }

    public void SetPatience(float current, float max)
    {
        if (fillImage == null || max <= 0f)
            return;

        float fill = Mathf.Clamp01(current / max);
        fillImage.fillAmount = fill;

        // Determine color based on thresholds
        if (fill <= lowThreshold)
        {
            originalColor = lowColor;
            currentPatience = PatienceLevel.Low;
            Flash();
        }
        else if (fill <= midThreshold)
        {
            originalColor = midColor;
            currentPatience = PatienceLevel.Mid;
            FlashForDur(2f);
        }
        else
        {
            originalColor = normalColor;
            currentPatience = PatienceLevel.Normal;
            fillImage.color = normalColor;
        }
    }

    private void Flash()
    {
        currentFlashInterval -= Time.deltaTime;

        if (currentFlashInterval <= 0)
        {
            if (fillImage.color == originalColor)
                fillImage.color = flashColor;
            else
                fillImage.color = originalColor;

            currentFlashInterval = flashInterval;
        }
    }

    private void FlashForDur(float dur)
    {
        // To prevent flash duration from being reset 
        if (!isFlashing)
        {
            isFlashing = true;
            flashDuration = dur;
        }

        if (flashDuration > 0)
        {
            flashDuration -= Time.deltaTime;

            Flash();
        }
        else
        {
            // Stop flash and reset color
            fillImage.color = originalColor;
        }
    }

    public void Show(bool visible)
    {
        gameObject.SetActive(visible);
    }
}