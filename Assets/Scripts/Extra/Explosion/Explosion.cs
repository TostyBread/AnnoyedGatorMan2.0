using UnityEngine;

public class Explosion : MonoBehaviour
{
    private float currentTime;
    private Vector3 initialScale;
    private float animationLength;

    [Header("Reference")]
    public Animator animator;
    public CapsuleCollider2D heatCollider;
    private CapsuleCollider2D capsuleCollider2D;
    private bool soundPlayed;

    void Start()
    {
        initialScale = transform.localScale;
        capsuleCollider2D = GetComponent<CapsuleCollider2D>();
        soundPlayed = false;

        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            animationLength = stateInfo.length > 0 ? stateInfo.length : 0.5f; // Default fallback
        }

        // Play explosion sound immediately when the explosion starts
        PlayExplosionSound();
    }

    void Update()
    {
        currentTime += Time.deltaTime;
        if (heatCollider != null) heatCollider.size = capsuleCollider2D.size;

        // Linearly scale the collider instead of exponential growth
        float scaleFactor = 1.2f * Time.deltaTime; // Adjust this as needed
        capsuleCollider2D.size += new Vector2(scaleFactor, scaleFactor);

        if (currentTime >= animationLength)
        {
            Destroy(gameObject);
        }
    }

    void PlayExplosionSound()
    {
        if (soundPlayed) return;

        // Add null check for AudioManager.Instance
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("Explosion", transform.position);
            soundPlayed = true;
        }
        else
        {
            Debug.LogWarning("AudioManager instance not found! Cannot play explosion sound.");
        }
    }
}