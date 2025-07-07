using UnityEngine;

public class Fist : MonoBehaviour
{
    private Animator animator;
    private Collider2D hitbox;

    [Header("Do not touch")]
    public bool isPunching = false;
    public bool isThrowing = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        hitbox = GetComponent<Collider2D>();
        hitbox.enabled = false;
    }

    public void TriggerPunch()
    {
        if (!gameObject.activeSelf || isThrowing) return;
        if (!isPunching)
        {
            Punch();
        }
    }

    private void Punch()
    {
        isPunching = true;
        AudioManager.Instance.PlaySound("slash1", 1.0f, transform.position);

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