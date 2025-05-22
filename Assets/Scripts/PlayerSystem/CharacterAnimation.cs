using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    private Animator animator;
    private CharacterMovement movementScript;
    private HealthManager healthManager;

    private bool dyingTriggered = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        movementScript = GetComponentInParent<CharacterMovement>();
        healthManager = GetComponentInParent<HealthManager>();
    }

    void Update()
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
            dyingTriggered = true;
            animator.SetBool("IsDying", true);
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
    }

    private void UpdateAnimationState()
    {
        animator.SetBool("IsMoving", movementScript.IsMoving);
    }
}