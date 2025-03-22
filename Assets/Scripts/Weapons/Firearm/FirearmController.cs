using UnityEngine;

public class FirearmController : MonoBehaviour, IUsable
{
    [Header("Firearm Settings")]
    public int maxAmmo = 10; // Maximum ammo the firearm can hold
    public GameObject projectilePrefab; // Prefab for the projectile
    public Transform muzzlePoint; // Where the projectile spawns
    public float projectileSpeed = 10f; // Speed of the projectile

    [Header("Animation Settings")]
    public string firearmPrefix = "Glock17"; // Prefix for firearm animations
    public Animator animator; // Reference to the Animator component

    [Header("References")]
    private CharacterFlip characterFlip; // Reference to the CharacterFlip script

    private int currentAmmo; // Current ammo count
    private bool isUsable = true; // Whether the firearm can currently be used
    private bool isFacingRight; // Tracks the player's current facing direction

    private bool isOutOfAmmo => currentAmmo <= 0; // Check if the firearm is out of ammo

    void Awake()
    {
        currentAmmo = maxAmmo;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator == null)
        {
            Debug.LogWarning($"Animator is missing on {gameObject.name}.");
        }
        else
        {
            PlayAnimation("_Neutral"); // Start in the neutral state
        }

        

        // Initialize facing direction
        isFacingRight = characterFlip != null && characterFlip.IsFacingRight();
        AdjustMuzzlePoint(); // Ensure muzzlePoint is set correctly at the start
    }

    void Update()
    {
        characterFlip = GetComponentInParent<CharacterFlip>();

        if (characterFlip == null) return;

        // Check if the player's facing direction has changed
        bool currentFacingRight = characterFlip.IsFacingRight();
        if (currentFacingRight != isFacingRight)
        {
            isFacingRight = currentFacingRight;
            AdjustMuzzlePoint(); // Only adjust when facing direction changes
        }
    }

    public void Use()
    {
        if (!isUsable)
        {
            Debug.Log("Firearm is currently disabled.");
            return;
        }

        if (isOutOfAmmo)
        {
            PlayAnimation("_Dry"); // Play the dry-fire animation
            AudioManager.Instance.PlaySound("dryfire", 1.0f, transform.position); // Plays gunshot at player's position
            Debug.Log("Out of ammo!");
            return;
        }

        if (currentAmmo == 1)
        {
            PlayOnce("_FireToDry", shouldGoToDry: true); // Last shot logic
            currentAmmo = 0; // Set ammo to zero
        }
        else
        {
            PlayOnce("_Fire"); // Regular firing animation
            currentAmmo--; // Decrease ammo
        }

        AudioManager.Instance.PlaySound("glock18", 1.0f, transform.position); // Plays gunshot at player's position

        SpawnProjectile();
        Debug.Log($"Fired! Ammo remaining: {currentAmmo}/{maxAmmo}");
    }

    public void EnableUsableFunction()
    {
        isUsable = true;
        Debug.Log("Firearm usable function enabled.");
    }

    public void DisableUsableFunction()
    {
        isUsable = false;
        Debug.Log("Firearm usable function disabled.");
    }

    private void SpawnProjectile()
    {
        if (projectilePrefab != null && muzzlePoint != null)
        {
            // Instantiate the projectile at the muzzle point
            GameObject projectile = Instantiate(projectilePrefab, muzzlePoint.position, muzzlePoint.rotation);

            // Set the velocity of the projectile
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Ensure velocity direction matches the muzzlePoint's local facing direction
                Vector2 direction = muzzlePoint.right.normalized; // Use the local "right" direction of the muzzle
                rb.velocity = direction * projectileSpeed;
            }
        }
    }

    private void AdjustMuzzlePoint()
    {
        if (characterFlip == null || muzzlePoint == null) return;

        // Check the player's facing direction
        bool isFacingRight = characterFlip.IsFacingRight();

        // Flip the muzzlePoint's local position based on the facing direction
        Vector3 muzzleLocalPosition = muzzlePoint.localPosition;
        muzzleLocalPosition.x = Mathf.Abs(muzzleLocalPosition.x) * (isFacingRight ? 1 : -1); // Flip X position
        muzzlePoint.localPosition = muzzleLocalPosition;

        // Flip the muzzlePoint's rotation based on the facing direction
        Vector3 muzzleRotation = muzzlePoint.localEulerAngles;
        muzzleRotation.y = isFacingRight ? 0 : 180; // Flip the Y-axis rotation
        muzzlePoint.localEulerAngles = muzzleRotation;
    }


    private void PlayOnce(string suffix, bool shouldGoToDry = false)
    {
        if (animator != null)
        {
            string animationName = firearmPrefix + suffix;
            animator.Play(animationName);

            // Calculate animation duration and schedule a state change
            float animationDuration = GetAnimationClipLength(animationName);
            if (shouldGoToDry)
            {
                Invoke(nameof(ResetToDry), animationDuration); // Transition to _Dry
            }
            else
            {
                Invoke(nameof(ResetToNeutral), animationDuration); // Transition to _Neutral
            }
        }
        else
        {
            Debug.LogWarning($"Animator is missing or not assigned for {gameObject.name}.");
        }
    }

    private void ResetToNeutral()
    {
        PlayAnimation("_Neutral"); // Return to neutral animation
    }

    private void ResetToDry()
    {
        PlayAnimation("_Dry"); // Stay in dry state when out of ammo
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