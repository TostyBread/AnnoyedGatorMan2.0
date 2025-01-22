using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    private Animator animator;
    private CharacterMovement movementScript;

    private bool isDead = false; // Placeholder for health logic
    private bool hasPlayedDeathAnimation = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        movementScript = GetComponentInParent<CharacterMovement>();
    }

    void Update()
    {
        if (isDead)
        {
            HandleDeathAnimation();
        }
        else
        {
            UpdateAnimationState();
        }
    }

    private void UpdateAnimationState()
    {
        if (movementScript.IsMoving)
        {
            animator.Play("Gator_MoveAnim");
        }
        else
        {
            animator.Play("Gator_IdleAnim");
        }
    }

    private void HandleDeathAnimation()
    {
        if (!hasPlayedDeathAnimation)
        {
            animator.Play("Gator_DeathAnim");
            hasPlayedDeathAnimation = true;
        }
    }

    public void Die()
    {
        isDead = true;
    }
}