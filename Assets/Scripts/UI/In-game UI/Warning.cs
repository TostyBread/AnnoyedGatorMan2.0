using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Warning : MonoBehaviour
{
    public Transform target; // the object to follow

    [Header("Heat to Flash")]
    [SerializeField] private float flashStartPercent = 0.7f;

    [Header("Flash Speed")]
    [SerializeField] private float maxFlashInterval = 1f;   // slow flashing
    [SerializeField] private float minFlashInterval = 0.1f; // fast flashing

    [Header("Reference")]
    public float flashInterval = 1f;
    private ItemSystem itemSystem;
    private ItemStateManager itemStateManager;

    private Image image;

    private void Start()
    {
        itemSystem = target.GetComponentInParent<ItemSystem>();
        itemStateManager = target.GetComponentInParent<ItemStateManager>();
        image = GetComponent<Image>();

        if (image != null)
        {
            image.enabled = false;
        }

        if (itemSystem == null || itemStateManager == null || target == null)
        {
            Destroy(gameObject);
            return;
        }

        StartCoroutine(FlashRoutine());
    }

    void Update()
    {
        if (itemSystem == null || itemStateManager == null || target == null || itemSystem.isOnPlate)
        {
            Destroy(gameObject);
            return;
        }

        // Convert world position to screen position
        transform.position = target.position;
    }

    private IEnumerator FlashRoutine()
    {
        while (true)
        {
            float t;

            if (!itemSystem.isCooked && !itemSystem.isBurned)
                t = Mathf.Clamp01(itemSystem.currentCookPoints / itemSystem.cookThreshold);
            else if (itemSystem.isCooked && !itemSystem.isBurned)
                t = Mathf.Clamp01(itemSystem.currentCookPoints / itemSystem.burnThreshold);
            else
                t = 1f;

            // If below threshold → wait and skip flashing
            if (t < flashStartPercent)
            {
                image.enabled = false;
                yield return null;
                continue;
            }

            // Normalize t so flashStartPercent == 0 and 1 == 1
            float tNormalized = Mathf.InverseLerp(flashStartPercent, 1f, t);

            // Flash faster the closer t is to 1
            float flashDelay = Mathf.Lerp(maxFlashInterval, minFlashInterval, tNormalized);

            // Toggle
            image.enabled = !image.enabled;

            if (image.enabled)
                AudioManager.Instance.PlaySound("beep1", transform.position);

            yield return new WaitForSeconds(flashDelay);         
        }
    }

}
