using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PatienceBarController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fillImage;

    [Header("Color Thresholds")]
    [SerializeField] private float midThreshold = 0.5f;
    [SerializeField] private float lowThreshold = 0.2f;

    [SerializeField] private Color normalColor = Color.green;
    [SerializeField] private Color midColor = Color.yellow;
    [SerializeField] private Color lowColor = Color.red;

    [Header("Flash")]
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashInterval = 0.1f;

    private Color originalColor;
    private float currentFlashInterval;

    private bool isFlashing = false;
    private float flashDuration;

    private void Start()
    {
        currentFlashInterval = flashInterval;
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
            Flash();
        }
        else if (fill <= midThreshold)
        {
            originalColor = midColor;
            FlashForDur(2f);
        }
        else
        {
            originalColor = normalColor;
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