using UnityEngine;

public class Fist : MonoBehaviour
{
    private Animator animator;
    private Collider2D hitbox;
    private DamageSource damageSource;
    private SpriteDeformationController deformer;

    [Header("Do not touch")]
    public bool isPunching = false;
    public bool isThrowing = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        hitbox = GetComponent<Collider2D>();
        deformer = GetComponent<SpriteDeformationController>();
        damageSource = GetComponent<DamageSource>();
        hitbox.enabled = false;
    }

    public void TriggerPunch()
    {
        if (!gameObject.activeSelf || isThrowing) return;
        if (!isPunching)
        {
            Punch();
            deformer?.TriggerSquash(0.5f, 5f, 0.4f, true);
        }
    }

    private void Punch()
    {
        isPunching = true;
        AudioManager.Instance.PlaySound("slash1", transform.position);

        if (gameObject.activeSelf)
        {
            animator.Play("PunchAnim");
            Invoke("EnableHitbox", 0.1f);
        }
    }

    private void EnableHitbox()
    {
        if (gameObject.activeSelf)
        {
            Physics2D.IgnoreCollision(damageSource.ownerCollider, hitbox, true); //disable the collision between player and player's punch

            hitbox.enabled = true;
            Invoke("DisableHitbox", 0.2f);
        }
    }

    private void DisableHitbox()
    {
        if (gameObject.activeSelf)
        {
            hitbox.enabled = false;
            isPunching = false;

            if (!isActiveAndEnabled || animator == null || !animator.isActiveAndEnabled) return;
            animator.Play("FistAnim");
        }
    }

    public void CancelPunch()
    {
        CancelInvoke();
        isPunching = false;
        if (hitbox != null) hitbox.enabled = false;
        if (animator != null && animator.isActiveAndEnabled)
            animator.Play("FistAnim");
    }
}