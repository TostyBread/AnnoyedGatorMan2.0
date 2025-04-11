// File: FirearmController.cs

using UnityEngine;
using System.Collections.Generic;

public class FirearmController : MonoBehaviour, IUsable
{
    public enum FireMode
    {
        Single,
        Auto,
        Shotgun
    }

    [Header("Firearm Settings")]
    public int maxAmmo = 10; // Maximum ammo the firearm can hold
    public GameObject projectilePrefab; // Prefab for the projectile
    public Transform muzzlePoint; // Where the projectile spawns
    public float projectileSpeed = 10f; // Speed of the projectile
    public float fireDelay = 0.25f; // Minimum time between shots (in seconds)

    [Header("Shotgun Settings")]
    public FireMode currentFireMode = FireMode.Single; // Current fire mode (Single or Shotgun)
    public int shotgunPelletCount = 5; // Number of projectiles fired in shotgun mode
    public float shotgunSpreadAngle = 15f; // Total spread angle for the shotgun
    public float shotgunRandomness = 5f; // Random angle added to each pellet (± value)

    [Header("Animation and Audio Settings")]
    public string firearmPrefix = "Glock17"; // Prefix for firearm animations
    public string firearmAudio = "glock18";
    public Animator animator; // Reference to the Animator component

    [Header("References")]
    public CharacterFlip characterFlip; // Reference to the CharacterFlip script

    private int currentAmmo; // Current ammo count
    private bool isUsable = true; // Whether the firearm can currently be used
    private bool isFacingRight; // Tracks the player's current facing direction
    private float nextFireTime = 0f; // Timestamp of when the gun can fire next
    private Dictionary<string, float> animationClipCache = new(); // Cache for animation clip lengths
    private Vector3 initialMuzzleLocalPosition; // Cache original muzzle local position

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
            CacheAnimationClipLengths(); // Preload animation clip lengths
        }

        // Initialize facing direction
        isFacingRight = characterFlip != null && characterFlip.IsFacingRight();

        AdjustMuzzlePoint(); // Ensure muzzlePoint is set correctly at the start
    }

    void Update()
    {
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

        // Enforce fire rate delay
        if (Time.time < nextFireTime)
        {
            Debug.Log("Waiting for fire delay cooldown.");
            return;
        }

        if (isOutOfAmmo)
        {
            PlayAnimation("_Dry"); // Play the dry-fire animation
            AudioManager.Instance.PlaySound("dryfire", 1.0f, transform.position); // Plays dry fire sound at player's position
            Debug.Log("Out of ammo!");
            return;
        }

        nextFireTime = Time.time + fireDelay; // Set next allowed fire time

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

        AudioManager.Instance.PlaySound(firearmAudio, 1.0f, transform.position); // Plays gunshot at player's position

        SpawnProjectile(); // Handle projectile spawning
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
        if (projectilePrefab == null || muzzlePoint == null) return;

        switch (currentFireMode)
        {
            case FireMode.Single:
            case FireMode.Auto:
                FireProjectile(muzzlePoint.rotation);
                break;

            case FireMode.Shotgun:
                // Fire multiple pellets with spread
                float startAngle = -shotgunSpreadAngle * 0.5f;
                float angleStep = shotgunSpreadAngle / (shotgunPelletCount - 1);

                for (int i = 0; i < shotgunPelletCount; i++)
                {
                    float baseAngle = startAngle + angleStep * i;

                    // Add random variation within ±shotgunRandomness degrees
                    float randomOffset = Random.Range(-shotgunRandomness, shotgunRandomness);

                    float totalAngle = baseAngle + randomOffset;

                    // Apply the angle offset around the Z axis
                    Quaternion spreadRotation = muzzlePoint.rotation * Quaternion.Euler(0, 0, totalAngle);
                    FireProjectile(spreadRotation);
                }
                break;
        }
    }

    private void FireProjectile(Quaternion rotation)
    {
        // Instantiate the projectile at the muzzle point
        GameObject projectile = Instantiate(projectilePrefab, muzzlePoint.position, rotation);

        // Set the velocity of the projectile
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Ensure velocity direction matches the rotated spread direction
            Vector2 direction = rotation * Vector2.right; // Adjust for spread rotation
            rb.velocity = direction.normalized * projectileSpeed;
        }
    }

    private void AdjustMuzzlePoint()
    {
        if (characterFlip == null || muzzlePoint == null) return;

        // Only flip muzzle rotation without modifying local position
        bool isFacingRight = characterFlip.IsFacingRight();

        Vector3 localEuler = muzzlePoint.localEulerAngles;
        localEuler.y = isFacingRight ? 0 : 180;
        muzzlePoint.localEulerAngles = localEuler;
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
            return;
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

    private void CacheAnimationClipLengths()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (!animationClipCache.ContainsKey(clip.name))
            {
                animationClipCache[clip.name] = clip.length;
            }
        }
    }

    private float GetAnimationClipLength(string animationName)
    {
        if (animationClipCache.TryGetValue(animationName, out float cachedLength))
        {
            return cachedLength;
        }

        Debug.LogWarning($"Animation clip '{animationName}' not found in cache for {gameObject.name}.");
        return 0f; // Default to 0 if not found
    }
}