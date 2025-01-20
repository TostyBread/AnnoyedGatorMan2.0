using UnityEngine;

public class Fist : MonoBehaviour
{
    private Animator animator;
    private Collider2D hitbox;
    private bool isPunching = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        hitbox = GetComponent<Collider2D>();
        hitbox.enabled = false; // Disable the hitbox initially
    }

    public void TriggerPunch()
    {
        // Ensure the fist GameObject is active before executing
        if (!gameObject.activeSelf)
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

        // Ensure the fist GameObject is still active before starting animation
        if (gameObject.activeSelf)
        {
            animator.Play("PunchAnim");
            Invoke("EnableHitbox", 0.1f); // Adjust timing to match animation
        }
    }

    private void EnableHitbox()
    {
        // Ensure the fist GameObject is still active before enabling the hitbox
        if (gameObject.activeSelf)
        {
            hitbox.enabled = true;
            Invoke("DisableHitbox", 0.2f); // Adjust timing to match animation
        }
    }

    private void DisableHitbox()
    {
        // Ensure the fist GameObject is still active before resetting
        if (gameObject.activeSelf)
        {
            hitbox.enabled = false;
            isPunching = false;

            // Return to idle animation
            animator.Play("FistAnim");
        }
    }
}