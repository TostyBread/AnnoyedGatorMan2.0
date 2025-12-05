using UnityEngine;
using UnityEngine.UI;

public class HeatBar : MonoBehaviour
{
    public Transform target; // the object to follow
    public Vector3 offset;   // vertical offset above target
    public Image bar;  // assign the fill image in inspector

    private ItemSystem itemSystem;
    private ItemStateManager itemStateManager;

    void Start()
    {
        itemSystem = target.GetComponentInParent<ItemSystem>();
        itemStateManager = target.GetComponentInParent<ItemStateManager>();

        if (bar != null) bar.fillAmount = 0f;

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    // COMMENT OUT DUE TO UNUSED FOR NOW

    private void Update()
    {
        if (itemSystem == null || bar == null || target == null || itemSystem.isOnPlate)
        {
            Destroy(gameObject);
            return;
        }

        // Convert world position to screen position
        transform.position = target.position + offset;

        float cookThreshold = itemSystem.cookThreshold;
        float burnThreshold = itemSystem.burnThreshold;
        float currentCookPoints = itemSystem.currentCookPoints;


        //Show or hide the bar based on fill amount
        if (bar.fillAmount > 0 && bar.fillAmount < 1)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }
        }
        else
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
        }

        // Update fill amount based on cooking state
        if (currentCookPoints <= cookThreshold)
        {
            bar.fillAmount = Mathf.Clamp01(currentCookPoints / cookThreshold);
        }
        else if (currentCookPoints <= burnThreshold)
        {
            bar.fillAmount = Mathf.Clamp01((currentCookPoints - cookThreshold) / (burnThreshold - cookThreshold));
        }
        else if (itemSystem.isBurned)
        {
            bar.fillAmount = Mathf.Clamp01(itemStateManager.currentHeat / itemStateManager.maxHeat);
        }
        else
        {
            bar.fillAmount = 1f;
        }
    }
}

