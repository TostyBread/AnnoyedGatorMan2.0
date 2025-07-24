using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    private Animator animator;
    private EnemyMovement enemyMovement;
    private HealthManager healthManager;
    public GameObject Enemy;
    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        enemyMovement = Enemy.GetComponent<EnemyMovement>();
        healthManager = Enemy.GetComponentInParent<HealthManager>();

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
        HandleAnimationFlip();

        //if (healthManager.currentHealth <= 0)  // Too much warning log, pls implement enemy death animation before uncommenting
        //{
        //    HandleDieAni();
        //}

        HandleMovingAni(enemyMovement.isMoving);
        HandleAttackAni(enemyMovement.isAttacking);
    }

    private void HandleAnimationFlip()
    {
        float zAngle = Enemy.transform.eulerAngles.z;

        if (zAngle > 90 && zAngle < 270)
            spriteRenderer.flipX = false;
        else
            spriteRenderer.flipX = true;
    }

    void HandleDieAni()
    {
        try
        {
            animator.SetBool("isDefeated", true);
        }
        catch
        {
            Debug.LogWarning("animation has no isDefeated boolean");
        }
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
