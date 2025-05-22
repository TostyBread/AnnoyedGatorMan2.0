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
        hitbox.enabled = false; // Disable the hitbox initially
    }

    public void TriggerPunch()
    {
        // Ensure the fist GameObject is active before executing
        if (!gameObject.activeSelf || isThrowing) // Prevent punching during throw animation
        {
            return;
        }

        if (!isPunching)
        {
            Punch();
        }
    }

    private void Punch()
    {
        isPunching = true;
        AudioManager.Instance.PlaySound("slash1", 1.0f, transform.position);

        // Ensure the fist GameObject is still active before starting animation
        if (gameObject.activeSelf)
        {
            animator.Play("PunchAnim");
            Invoke("EnableHitbox", 0.1f); // Adjust timing to match animation
        }
    }

    private void EnableHitbox()
    {
        if (gameObject.activeSelf)
        {
            hitbox.enabled = true;
            Invoke("DisableHitbox", 0.2f); // Adjust timing to match animation
        }
    }

    private void DisableHitbox()
    {
        if (gameObject.activeSelf)
        {
            hitbox.enabled = false;
            isPunching = false;

            if (!isActiveAndEnabled || animator == null || !animator.isActiveAndEnabled) return; //Safety measure just in case player still tries to punch during split-transition to KO state
            // Return to idle animation
            animator.Play("FistAnim");
        }
    }
}