using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public Animator animator;

    private float currentTime;
    private Vector3 initialScale;
    private Vector2 initialColliderSize;
    private CapsuleCollider2D capsuleCollider2D;
    private float animationLength;

    void Start()
    {
        initialScale = transform.localScale;
        capsuleCollider2D = GetComponent<CapsuleCollider2D>();

        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            animationLength = stateInfo.length > 0 ? stateInfo.length : 0.5f; // Default fallback
        }

        AudioManager.Instance.PlaySound("Explosion", 1.0f, transform.position);
    }

    void Update()
    {
        currentTime += Time.deltaTime;

        // Linearly scale the collider instead of exponential growth
        float scaleFactor = 1.2f * Time.deltaTime; // Adjust this as needed
        capsuleCollider2D.size += new Vector2(scaleFactor, scaleFactor);

        if (currentTime >= animationLength)
        {
           Destroy(gameObject);
        }
    }
}