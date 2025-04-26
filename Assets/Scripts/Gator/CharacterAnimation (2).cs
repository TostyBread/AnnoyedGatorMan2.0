using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    private Animator animator;
    private CharacterMovement movementScript;

    private HealthManager healthManager;

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
            animator.SetBool("IsDead", true);
        }
        else
        {
            UpdateAnimationState();
        }
    }

    private void UpdateAnimationState()
    {
        animator.SetBool("IsDead", false);

        if (movementScript.IsMoving)
        {
            animator.Play("Gator_MoveAnim");
        }
        else
        {
            animator.Play("Gator_IdleAnim");
        }
    }
}