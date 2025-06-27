using UnityEngine;
using UnityEngine.UI;

public class PatienceBarController : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    public void SetPatience(float current, float max)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01(current / max);
        }
    }

    public void Show(bool visible)
    {
        gameObject.SetActive(visible);
    }
}