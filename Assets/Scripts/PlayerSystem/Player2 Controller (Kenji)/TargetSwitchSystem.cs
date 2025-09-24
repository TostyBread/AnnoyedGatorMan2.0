using UnityEngine;
using System.Collections.Generic;

public class TargetSwitchSystem : MonoBehaviour
{
    private PlayerInputManagerP2 inputManager;
    private PlayerPickupSystemP2 pickupSystem;
    private PlayerAimController aimController;
    private int currentTargetIndex = -1;
    private List<Collider2D> targetsInRange = new List<Collider2D>();

    void Start()
    {
        inputManager = GetComponent<PlayerInputManagerP2>();
        pickupSystem = GetComponent<PlayerPickupSystemP2>();
        aimController = PlayerAimController.Instance;

        // Subscribe to target switch events
        inputManager.OnNextTarget += SwitchToNextTarget;
        inputManager.OnPreviousTarget += SwitchToPreviousTarget;
    }

    void OnDestroy()
    {
        if (inputManager != null)
        {
            inputManager.OnNextTarget -= SwitchToNextTarget;
            inputManager.OnPreviousTarget -= SwitchToPreviousTarget;
        }
    }

    private void SwitchToNextTarget()
    {
        UpdateTargetsInRange();
        if (targetsInRange.Count == 0) return;

        currentTargetIndex = (currentTargetIndex + 1) % targetsInRange.Count;
        UpdateAimTarget();
    }

    private void SwitchToPreviousTarget()
    {
        UpdateTargetsInRange();
        if (targetsInRange.Count == 0) return;

        currentTargetIndex--;
        if (currentTargetIndex < 0) currentTargetIndex = targetsInRange.Count - 1;
        UpdateAimTarget();
    }

    private void UpdateTargetsInRange()
    {
        targetsInRange.Clear();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, pickupSystem.pickupRadius);

        HashSet<Collider2D> uniqueTargets = new HashSet<Collider2D>();

        foreach (var collider in colliders)
        {
            // Add pickupable items
            if (pickupSystem.validTags.Contains(collider.tag))
            {
                uniqueTargets.Add(collider);
            }
            // Add interactables
            else if (collider.gameObject.GetComponent<Interactable>() != null)
            {
                uniqueTargets.Add(collider);
            }
        }

        targetsInRange.AddRange(uniqueTargets);

        // Reset index if no targets or if current target is out of range
        if (targetsInRange.Count == 0 || currentTargetIndex >= targetsInRange.Count)
        {
            currentTargetIndex = -1;
        }
    }

    private void UpdateAimTarget()
    {
        if (currentTargetIndex >= 0 && currentTargetIndex < targetsInRange.Count)
        {
            Vector2 targetPosition = targetsInRange[currentTargetIndex].transform.position;
            aimController.ClearLockOn();
            aimController.LockOnToTarget(targetsInRange[currentTargetIndex].transform);
        }
    }
}
