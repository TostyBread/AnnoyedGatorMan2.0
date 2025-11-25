using UnityEngine;
using UnityEngine.UI;

public class Flash : MonoBehaviour
{
    private Image bar;

    [Header("Flash")]
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.5f;

    private Color originalColor;
    private float currentFlashDuration;

    private void Start()
    {
        bar = GetComponent<Image>();

        currentFlashDuration = flashDuration;
    }


    private void Update()
    {
        if (currentFlashDuration > 0)
        {
            currentFlashDuration -= Time.deltaTime;
            if (currentFlashDuration <= 0)
            {
                //if (bar.enabled == true)
                //{
                //    bar.enabled = false;
                //}
                //else
                //{
                //    bar.enabled = true;
                //}

                if (bar.color == originalColor)
                {
                    bar.color = flashColor;
                }
                else
                {
                    bar.color = originalColor;
                }

                currentFlashDuration = flashDuration;
            }
        }
    }
}
