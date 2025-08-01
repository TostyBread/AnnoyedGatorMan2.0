using UnityEngine;
using System.Collections.Generic;

public class FirearmController : MonoBehaviour, IUsable
{
    public enum FireMode { Single, Auto, Shotgun }

    [Header("Firearm Settings")]
    public int maxAmmo = 10;
    public GameObject projectilePrefab;
    public Transform muzzlePoint;
    public float projectileSpeed = 10f;
    public float fireDelay = 0.25f;

    [Header("Shotgun Settings")]
    public FireMode currentFireMode = FireMode.Single;
    public int shotgunPelletCount = 5;
    public float shotgunSpreadAngle = 15f;
    public float shotgunRandomness = 5f;

    [Header("Animation and Audio Settings")]
    public string firearmPrefix = "Glock17";
    public string firearmAudio = "glock18";
    public Animator animator;
    public bool isFiring = false;

    private int currentAmmo;
    [Header("Do not touch")]
    [SerializeField] private bool isUsable = true;
    private bool isFacingRight;
    private float nextFireTime = 0f;
    private Dictionary<string, float> animationClipCache = new();

    private ICharacterFlip ownerFlip;
    private Vector3 initialMuzzleLocalPosition;

    [Header("Who is holding current weapon")]
    [SerializeField]public GameObject owner; //Refrence from PickUp (PlayerPickupSystem or P2PickSystem)

    private bool isOutOfAmmo => currentAmmo <= 0;
    private bool hasPlayedDryFire = false;

    // Ammo refill references

    void Awake()
    {
        currentAmmo = maxAmmo;
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            PlayAnimation("_Neutral");
            CacheAnimationClipLengths();
        }
    }

    void Update()
    {
        if (ownerFlip == null) return;

        bool currentFacingRight = ownerFlip.IsFacingRight();
        if (currentFacingRight != isFacingRight)
        {
            isFacingRight = currentFacingRight;
            AdjustMuzzlePoint();
        }
    }

    public interface ICharacterFlip
    {
        bool IsFacingRight();
    }

    public void SetOwner(GameObject newOwner)
    {
        owner = newOwner;
        ownerFlip = newOwner.GetComponent<ICharacterFlip>();
        isFacingRight = ownerFlip != null && ownerFlip.IsFacingRight();
        AdjustMuzzlePoint();
    }

    public void ClearOwner()
    {
        owner = null;
        ownerFlip = null;
    }

    public void OnFireKeyReleased()
    {
        hasPlayedDryFire = false;
    }

    public void Use()
    {
        if (!isUsable || Time.time < nextFireTime) return;

        if (isOutOfAmmo)
        {
            if (!hasPlayedDryFire)
            {
                PlayAnimation("_Dry");
                AudioManager.Instance.PlaySound("dryfire", transform.position);
                hasPlayedDryFire = true;
            }
            return;
        }

        nextFireTime = Time.time + fireDelay;
        hasPlayedDryFire = false;

        if (currentAmmo == 1)
        {
            PlayOnce("_FireToDry", shouldGoToDry: true);
            currentAmmo = 0;
        }
        else
        {
            PlayOnce("_Fire");
            currentAmmo--;
        }

        AudioManager.Instance.PlaySound(firearmAudio, transform.position);
        SpawnProjectile();
    }

    public void EnableUsableFunction() => isUsable = true;
    public void DisableUsableFunction() => isUsable = false;
    public void ToggleUsableMode(bool enabled) => isUsable = enabled;
    public bool IsInUsableMode() => isUsable;

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
                float startAngle = -shotgunSpreadAngle * 0.5f;
                float angleStep = shotgunSpreadAngle / (shotgunPelletCount - 1);
                for (int i = 0; i < shotgunPelletCount; i++)
                {
                    float baseAngle = startAngle + angleStep * i;
                    float randomOffset = Random.Range(-shotgunRandomness, shotgunRandomness);
                    float totalAngle = baseAngle + randomOffset;
                    Quaternion spreadRotation = muzzlePoint.rotation * Quaternion.Euler(0, 0, totalAngle);
                    FireProjectile(spreadRotation);
                }
                break;
        }
    }

    private void FireProjectile(Quaternion rotation)
    {
        GameObject projectile = Instantiate(projectilePrefab, muzzlePoint.position, rotation);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 direction = rotation * Vector2.right;
            rb.velocity = direction.normalized * projectileSpeed;
        }

        DamageSource projectileDamageSource = projectile.GetComponent<DamageSource>();
        if (projectileDamageSource != null)
        {
            projectileDamageSource.SetOwner(owner); // Set the owner to the player
        }
    }

    private void AdjustMuzzlePoint()
    {
        if (ownerFlip == null || muzzlePoint == null) return;
        bool isFacingRight = ownerFlip.IsFacingRight();
        Vector3 localEuler = muzzlePoint.localEulerAngles;
        localEuler.y = isFacingRight ? 0 : 180;
        muzzlePoint.localEulerAngles = localEuler;
    }

    private void PlayOnce(string suffix, bool shouldGoToDry = false)
    {
        isFiring = true;
        if (animator == null) return;
        string animationName = firearmPrefix + suffix;
        animator.Play(animationName);
        float animationDuration = GetAnimationClipLength(animationName);
        Invoke(shouldGoToDry ? nameof(ResetToDry) : nameof(ResetToNeutral), animationDuration);
    }

    private void ResetToNeutral()
    {
        PlayAnimation("_Neutral");
        isFiring = false;
    }

    private void ResetToDry()
    {
        PlayAnimation("_Dry");
        isFiring = false;
    }

    // Reference from AmmoBox for refilling ammo
    public void RefillFull()
    {
        currentAmmo = maxAmmo;
        hasPlayedDryFire = false;
        ResetToNeutral();
    }

    public void RefillAmmo(int amount)
    {
        currentAmmo = Mathf.Min(maxAmmo, currentAmmo + amount);
        hasPlayedDryFire = false;
        ResetToNeutral();
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
        return 0f;
    }
}