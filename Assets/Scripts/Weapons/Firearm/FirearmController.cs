using UnityEngine;

public class FirearmController : MonoBehaviour
{
    [Header("Firearm Settings")]
    public int maxAmmo = 10; // Maximum ammo the firearm can hold
    public GameObject projectilePrefab; // Prefab for the projectile
    public Transform muzzlePoint; // Where the projectile spawns
    public float projectileSpeed = 10f; // Speed of the projectile

    [Header("Animation Settings")]
    public string firearmPrefix = "Glock17"; // Prefix for firearm animations
    public Animator animator; // Reference to the Animator component

    private int currentAmmo; // Current ammo count

    private bool isOutOfAmmo => currentAmmo <= 0;

    void Awake()
    {
        // Initialize ammo count
        currentAmmo = maxAmmo;

        // If the animator is not manually assigned, try finding it in children
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator == null)
        {
            Debug.LogWarning($"Animator is missing on {gameObject.name} or its children.");
        }
        else
        {
            PlayAnimation("_Neutral"); // Set to the neutral animation at the start
        }
    }

    public void Fire()
    {
        if (isOutOfAmmo)
        {
            PlayAnimation("_Dry"); // Play the DryFire animation
            Debug.Log("Out of ammo!");
            return;
        }

        if (currentAmmo == 1)
        {
            PlayOnce("_FireToDry"); // Last-shot animation
            currentAmmo = 0; // Ammo depleted
        }
        else
        {
            PlayOnce("_Fire"); // Regular fire animation
            currentAmmo--;
        }

        SpawnProjectile();
        Debug.Log($"Fired! Ammo remaining: {currentAmmo}/{maxAmmo}");
    }

    private void SpawnProjectile()
    {
        if (projectilePrefab != null && muzzlePoint != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, muzzlePoint.position, muzzlePoint.rotation);
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = muzzlePoint.right * projectileSpeed; // Forward direction based on the muzzle
            }
        }
    }

    private void PlayOnce(string suffix)
    {
        if (animator != null)
        {
            string animationName = firearmPrefix + suffix;
            animator.Play(animationName);

            // Return to _Neutral after the animation finishes
            float animationDuration = GetAnimationClipLength(animationName);
            Invoke(nameof(ResetToNeutral), animationDuration);
        }
        else
        {
            Debug.LogWarning($"Animator is missing or not assigned for {gameObject.name}.");
        }
    }

    private void ResetToNeutral()
    {
        PlayAnimation("_Neutral");
    }

    private void PlayAnimation(string suffix)
    {
        if (animator != null)
        {
            string animationName = firearmPrefix + suffix;
            animator.Play(animationName);
        }
    }

    private float GetAnimationClipLength(string animationName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"Cannot get animation length. Animator is missing or not properly set up for {gameObject.name}.");
            return 0f;
        }

        // Search through all animation clips in the runtime controller to find the matching clip
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == animationName)
            {
                return clip.length;
            }
        }

        Debug.LogWarning($"Animation clip '{animationName}' not found for {gameObject.name}.");
        return 0f; // Default to 0 if not found
    }
}