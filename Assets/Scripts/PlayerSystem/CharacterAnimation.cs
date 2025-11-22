using System;
using System.Collections;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    private Animator animator;
    private CharacterMovement movementScript;
    private HealthManager healthManager;
    private Collider2D playerCollider; // collider

    private PlayerPickupSystem playerPickupSystem;
    private P2PickupSystem p2PickSystem;

    private bool dyingTriggered = false;

    void Start()
    {
        playerCollider = GetComponentInParent<HealthManager>().GetComponent<BoxCollider2D>(); // Specifically reference parent object and skipping child
        animator = GetComponent<Animator>();
        movementScript = GetComponentInParent<CharacterMovement>();
        healthManager = GetComponentInParent<HealthManager>();

        playerPickupSystem = GetComponentInParent<PlayerPickupSystem>();
        if (playerPickupSystem == null)
        {
            p2PickSystem = GetComponentInParent<P2PickupSystem>();
        }
    }

    void FixedUpdate()
    {
        if (healthManager.currentHealth <= 0)
        {
            HandleDeathTransition();
        }
        else
        {
            ResetDeathStates();
            UpdateAnimationState();
        }
    }

    private void HandleDeathTransition()
    {
        if (!dyingTriggered)
        {
            animator.SetBool("IsHurt", false);
            dyingTriggered = true;
            animator.SetBool("IsDying", true);
            StartCoroutine(DisableColliderNextFrame());
        }
    }

    // Called via animation event at end of dying animation
    public void OnDyingAnimationComplete()
    {
        animator.SetBool("IsDying", false);
        animator.SetBool("IsDead", true);
    }

    private void ResetDeathStates()
    {
        animator.SetBool("IsDying", false);
        animator.SetBool("IsDead", false);
        dyingTriggered = false;
        playerCollider.enabled = true;
    }

    private void UpdateAnimationState()
    {
        animator.SetBool("IsMoving", movementScript.IsMoving);

        try
        {
            if (playerPickupSystem) animator.SetBool("IsSmoking", playerPickupSystem.isSmoking);
            else if (p2PickSystem) animator.SetBool("IsSmoking", p2PickSystem.isSmoking);
        }
        catch (NullReferenceException)
        {
            //Debug.LogWarning(gameObject.name + " has no IsSmoking animation");
        }
    
        animator.SetBool("IsHurt", healthManager.isHurt);
    }

    private IEnumerator DisableColliderNextFrame()
    {
        yield return null; // wait 1 frame until Rigidbody2D rebuild finishes
        playerCollider.enabled = false;
    }

}