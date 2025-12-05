using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    private Animator animator;
    private EnemyMovement enemyMovement;
    private HealthManager healthManager;
    public GameObject Enemy;
    private SpriteRenderer spriteRenderer;

    Vector2 previousPos;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        enemyMovement = Enemy.GetComponent<EnemyMovement>();
        healthManager = Enemy.GetComponentInParent<HealthManager>();

        spriteRenderer = GetComponent<SpriteRenderer>();

        previousPos = enemyMovement.TargetPos.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
        HandleAnimationFlip();
        //AimForTargetXPos();

        //if (healthManager.currentHealth <= 0)  // Too much warning log, pls implement enemy death animation before uncommenting
        //{
        //    HandleDieAni();
        //}

        HandleMovingAni(enemyMovement.isMoving);
        HandleAttackAni(enemyMovement.pauseAttackAni);
    }

    private void HandleAnimationFlip()
    {
        float zAngle = Enemy.GetComponentInChildren<GetMeFormOtherCode>().gameObject.transform.eulerAngles.z;

        if (zAngle > 90 && zAngle < 270)
        {
            spriteRenderer.flipX = false;
        }
        else
        {
            spriteRenderer.flipX = true;
        }
    }

    void ChangeDirection(int direction)
    { 
        Vector3 temp = Enemy.transform.localScale;

        if (direction == 1)
        {
            temp.x = Mathf.Abs(temp.x);
        }
        else if (direction == -1)
        {
            temp.x = -Mathf.Abs(temp.x);
        }
        Enemy.transform.localScale = temp;
    }

    void AimForTargetXPos()
    { 
        previousPos = enemyMovement.TargetPos.transform.position - transform.position;

        if (previousPos.x > 0)
        {
            ChangeDirection(1);
        }
        else
        { 
            ChangeDirection(-1);
        }

        previousPos = enemyMovement.TargetPos.transform.position;
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
        animator.SetBool("isAttacking", Attacking && spriteRenderer.flipX == false);
        animator.SetBool("isAttackingLeft", Attacking && spriteRenderer.flipX == true);
    }
}
