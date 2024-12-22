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

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isPunching)
        {
            Punch();
        }
    }

    private void Punch()
    {
        isPunching = true;
        animator.Play("PunchAnim");
        Invoke("EnableHitbox", 0.1f); // Adjust timing to match animation
    }

    // Enable the hitbox for the duration of the punch
    private void EnableHitbox()
    {
        hitbox.enabled = true;
        Invoke("DisableHitbox", 0.2f); // Adjust timing to match animation
    }

    private void DisableHitbox()
    {
        hitbox.enabled = false;
        isPunching = false;
        animator.Play("FistAnim"); // Return to idle animation
    }
}