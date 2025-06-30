using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class EnemyAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private EnemyMovement enemyMovement;
    [SerializeField] private HealthManager healthManager;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        enemyMovement = GetComponentInParent<EnemyMovement>();
        healthManager = GetComponentInParent<HealthManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (healthManager.currentHealth <= 0)
        {
            HandleDieAni();
        }

        HandleMovingAni(enemyMovement.isMoving);
        HandleAttackAni(enemyMovement.isAttacking);
    }

    void HandleDieAni()
    {
        animator.SetBool("isDefeated",true);
    }

    void HandleMovingAni(bool Moving)
    {
        animator.SetBool("isMoving", Moving);
    }

    void HandleAttackAni(bool Attacking)
    {
        animator.SetBool("isAttacking",Attacking);
    }
}
