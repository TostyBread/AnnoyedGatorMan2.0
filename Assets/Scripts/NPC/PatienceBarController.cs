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

    public void SetPatience(float current, float max)
    {
        if (fillImage == null || max <= 0f)
            return;

        float fill = Mathf.Clamp01(current / max);
        fillImage.fillAmount = fill;

        if (fill <= lowThreshold)
        {
            fillImage.color = lowColor;
        }
        else if (fill <= midThreshold)
        {
            fillImage.color = midColor;
        }
        else
        {
            fillImage.color = normalColor;
        }
    }

    public void Show(bool visible)
    {
        gameObject.SetActive(visible);
    }
}