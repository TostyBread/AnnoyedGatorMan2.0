using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{

    private float currentTime;
    private Vector3 initialScale;
    private Vector2 initialColliderSize;
    private float animationLength;

    private AudioSource explodeSound;
    private bool isPlayingSound = false;

    [Header("Reference")]
    public Animator animator;
    public CapsuleCollider2D heatCollider;
    private CapsuleCollider2D capsuleCollider2D;

    void Start()
    {
        initialScale = transform.localScale;
        capsuleCollider2D = GetComponent<CapsuleCollider2D>();

        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            animationLength = stateInfo.length > 0 ? stateInfo.length : 0.5f; // Default fallback
        }

        explodeSound = GetComponent<AudioSource>();
    }

    void Update()
    {
        currentTime += Time.deltaTime;
        if (heatCollider != null) heatCollider.size = capsuleCollider2D.size;

        // Linearly scale the collider instead of exponential growth
        float scaleFactor = 1.2f * Time.deltaTime; // Adjust this as needed
        capsuleCollider2D.size += new Vector2(scaleFactor, scaleFactor);

        PlayExplodeSoundOnce();

        if (currentTime >= animationLength)
        {
           Destroy(gameObject);
        }
    }

    private void PlayExplodeSoundOnce()
    {
        if (!isPlayingSound)
        {
            explodeSound.Play();
            isPlayingSound = true;
        }
    }
}