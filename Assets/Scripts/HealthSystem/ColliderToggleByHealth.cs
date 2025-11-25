using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ColliderToggleByHealth : MonoBehaviour
{
    private HealthManager healthManager;
    private Collider2D col;

    // Track last state to avoid repeating enable/disable
    private bool? lastState = null;

    void Start()
    {
        healthManager = GetComponent<HealthManager>();
        col = GetComponent<Collider2D>();

        //if (healthManager == null)
        //{
        //    Debug.LogError("ColliderToggleByHealth: No HealthManager found on this GameObject.");
        //}
    }

    void Update()
    {
        if (healthManager == null) return;

        bool shouldEnable = healthManager.currentHealth > 0 && !healthManager.isDefeated;

        // Only update when state changes
        if (!lastState.HasValue || lastState.Value != shouldEnable)
        {
            col.enabled = shouldEnable;
            lastState = shouldEnable;
        }
    }
}