using System.Collections;
using UnityEngine;

public class NPCAngerBehavior : MonoBehaviour
{
    public enum ShoutMode { Single, Burst, RapidFire, BigProjectile }

    [Header("Anger Config")]
    public int hitsToTriggerEscape = 3;
    public float shoutCooldown = 1.5f;
    public GameObject shoutProjectilePrefab;
    public Transform shoutSpawnPoint;
    public float projectileSpeed = 5f;
    public Color blinkColor = Color.red;
    public float blinkDuration = 0.15f;
    public int blinkCount = 3;
    [Range(0f, 1f)] public float angerChanceOnHit = 0.5f;
    
    [Header("Anger Timer")]
    [Tooltip("Maximum duration (in seconds) for anger mode before auto-exit")]
    public float angerDuration = 40f;
    [Tooltip("Should the NPC escape when anger timer expires?")]
    public bool escapeOnTimeout = false;
    
    [Header("Performance Settings")]
    [Tooltip("How often to refresh the player list (in seconds)")]
    public float playerListRefreshRate = 2f;
    [Tooltip("How often to recalculate closest player during rapid fire (every N shots)")]
    public int targetUpdateFrequency = 3;

    [Header("Shout Mode Weights (percent or relative weight)")]
    [Range(0f, 1f)] public float singleWeight = 0.25f;
    [Range(0f, 1f)] public float burstWeight = 0.25f;
    [Range(0f, 1f)] public float rapidFireWeight = 0.25f;
    [Range(0f, 1f)] public float bigProjectileWeight = 0.25f;

    private int hitCount = 0;
    private bool isAngry = false;
    private Coroutine shoutRoutine;
    private Coroutine angerTimerRoutine;
    private ShoutMode shoutMode;
    private NPCBehavior npc;
    private bool hasShoutModeSet = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Collider2D npcCollider;
    private GameObject[] allPlayers;
    private NPCAnimationController npcAnim; // Cache the component
    private SpriteDeformationController deformer; // deformer reference

    // Performance optimization: Cache frequently used values
    private GameObject currentTarget;
    private float lastPlayerRefreshTime;
    private Vector3 lastNPCPosition;
    
    // Cache WaitForSeconds to avoid allocation
    private WaitForSeconds blinkWait;
    private WaitForSeconds shoutCooldownWait;
    private WaitForSeconds burstWait;
    private WaitForSeconds rapidFireWait;

    public bool IsAngry => isAngry;
    public float RemainingAngerTime { get; private set; }

    void Awake()
    {
        // Search for deformer
        deformer = GetComponent<SpriteDeformationController>();

        if (deformer == null)
        {
            deformer = GetComponentInChildren<SpriteDeformationController>();
        }

        npc = GetComponent<NPCBehavior>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        npcCollider = GetComponent<Collider2D>();
        npcAnim = GetComponentInChildren<NPCAnimationController>(); // Cache the component
        
        // Find all players at start
        RefreshPlayerList();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        // Cache WaitForSeconds objects to avoid allocation during coroutines
        blinkWait = new WaitForSeconds(blinkDuration);
        shoutCooldownWait = new WaitForSeconds(shoutCooldown);
        burstWait = new WaitForSeconds(0.3f);
        rapidFireWait = new WaitForSeconds(0.2f);
    }

    private void RefreshPlayerList()
    {
        // Get all players with "Player" tag
        allPlayers = GameObject.FindGameObjectsWithTag("Player");
        lastPlayerRefreshTime = Time.time;
    }

    private GameObject GetClosestPlayer()
    {
        // Refresh player list periodically instead of every call
        if (Time.time - lastPlayerRefreshTime > playerListRefreshRate)
        {
            RefreshPlayerList();
        }

        if (allPlayers == null || allPlayers.Length == 0)
        {
            RefreshPlayerList(); // Try to refresh if list is empty
            if (allPlayers.Length == 0) return null;
        }

        GameObject closestPlayer = null;
        float closestDistanceSqr = float.MaxValue; // Use squared distance for better performance

        // Cache position to avoid multiple transform.position calls
        lastNPCPosition = transform.position;

        foreach (GameObject player in allPlayers)
        {
            if (player == null) continue; // Skip null references

            // Use sqrMagnitude for better performance (avoids square root calculation)
            float distanceSqr = (player.transform.position - lastNPCPosition).sqrMagnitude;
            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }

    public void TriggerAngerMode(GameObject source)
    {
        if (isAngry) return;
        if (npc.hasAcceptedPlate) return; // Don't trigger anger if the NPC has accepted a plate

        // Use cached component reference
        if (npcAnim != null)
        {
            npcAnim.IsAngry = true;
        }
        isAngry = true;
        RemainingAngerTime = angerDuration;

        if (npcCollider != null)
            npcCollider.enabled = true;

        if (!hasShoutModeSet)
        {
            shoutMode = GetWeightedShoutMode();
            hasShoutModeSet = true;
        }

        if (shoutRoutine != null)
            StopCoroutine(shoutRoutine);
        shoutRoutine = StartCoroutine(ShoutingLoop());

        // Start the anger timer
        if (angerTimerRoutine != null)
            StopCoroutine(angerTimerRoutine);
        angerTimerRoutine = StartCoroutine(AngerTimerRoutine());
    }

    private ShoutMode GetWeightedShoutMode()
    {
        float total = singleWeight + burstWeight + rapidFireWeight + bigProjectileWeight;

        if (total <= 0f)
        {
            // Fallback: avoid divide-by-zero, pick random
            return (ShoutMode)Random.Range(0, 4);
        }

        // Normalize into 0–1 range
        float roll = Random.value * total;

        if (roll < singleWeight)
            return ShoutMode.Single;

        roll -= singleWeight;
        if (roll < burstWeight)
            return ShoutMode.Burst;

        roll -= burstWeight;
        if (roll < rapidFireWeight)
            return ShoutMode.RapidFire;

        return ShoutMode.BigProjectile;
    }

    public void RegisterHit(GameObject source)
    {
        if (!isAngry)
        {
            if (Random.value < angerChanceOnHit)
                TriggerAngerMode(source);
        }
        else
        {
            hitCount++;
            //Debug.Log($"NPC {npc.customerId} hit count: {hitCount}/{hitsToTriggerEscape}");
            if (hitCount >= hitsToTriggerEscape)
            {
                ExitAngerMode();
                npc.ForceEscape();
            }
        }
    }

    private void ExitAngerMode()
    {
        if (!isAngry) return; // Already exited

        // Stop all anger-related coroutines first
        if (shoutRoutine != null)
        {
            StopCoroutine(shoutRoutine);
            shoutRoutine = null;
        }

        if (angerTimerRoutine != null)
        {
            StopCoroutine(angerTimerRoutine);
            angerTimerRoutine = null;
        }

        // Reset all anger-related state
        isAngry = false;
        hasShoutModeSet = false;
        hitCount = 0; // Reset hit count for future anger episodes
        RemainingAngerTime = 0f;

        // Reset animation state
        if (npcAnim != null)
        {
            npcAnim.IsAngry = false;
        }

        // Reset visual state
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        // Keep the NPC's rigidbody dynamic for normal movement
        if (npc != null && npc.rb != null)
            npc.rb.bodyType = RigidbodyType2D.Dynamic;
    }

    private IEnumerator AngerTimerRoutine()
    {
        float timer = angerDuration;
        
        while (timer > 0f && isAngry)
        {
            RemainingAngerTime = timer;
            timer -= Time.deltaTime;
            yield return null;
        }

        // Timer expired - ALWAYS force escape when anger duration ends
        if (isAngry)
        {
            //Debug.Log($"NPC {npc.customerId} anger timer expired - forcing escape");
            ExitAngerMode();
            npc.ForceEscape();
        }
    }

    private IEnumerator ShoutingLoop()
    {
        while (isAngry)
        {
            // Get the closest player each cycle
            currentTarget = GetClosestPlayer();
            
            // If no players are found, stop shooting
            if (currentTarget == null)
            {
                yield return shoutCooldownWait;
                continue;
            }

            if (spriteRenderer != null)
            {
                for (int i = 0; i < blinkCount; i++)
                {
                    if (!isAngry) yield break; // Exit if anger ended during blinking
                    
                    spriteRenderer.color = blinkColor;
                    yield return blinkWait;
                    
                    if (!isAngry) yield break; // Exit if anger ended during blinking
                    
                    spriteRenderer.color = originalColor;
                    yield return blinkWait;
                }

                // Flip direction to face the closest player
                if (currentTarget.transform.position.x < transform.position.x)
                    spriteRenderer.flipX = true;
                else
                    spriteRenderer.flipX = false;
            }

            if (!isAngry) yield break; // Exit if anger ended

            AudioManager.Instance.PlaySound("Swear", transform.position); // NPC swearing

            if (deformer != null)
            {
                deformer.TriggerDeformation(0f, 0f, 0f, 2f, 0.8f, 6f, 1.2f);
            }

            Vector2 dir = (currentTarget.transform.position - transform.position).normalized;
            switch (shoutMode)
            {
                case ShoutMode.Single:
                    FireProjectile(dir);
                    break;
                case ShoutMode.Burst:
                    yield return StartCoroutine(ExecuteBurstAttack());
                    break;
                case ShoutMode.RapidFire:
                    yield return StartCoroutine(ExecuteRapidFireAttack());
                    break;
                case ShoutMode.BigProjectile:
                    FireProjectile(dir, 2f);
                    break;
            }

            if (!isAngry) yield break; // Exit if anger ended during attack

            yield return shoutCooldownWait;
        }

        //Debug.Log($"NPC {npc.customerId} shouting loop ended");
    }

    private IEnumerator ExecuteBurstAttack()
    {
        for (int i = 0; i < 3; i++)
        {
            if (!isAngry) yield break; // Exit if anger ended

            // Only recalculate target every few shots for performance
            if (i % targetUpdateFrequency == 0 || currentTarget == null)
            {
                currentTarget = GetClosestPlayer();
            }
            
            if (currentTarget != null)
            {
                Vector2 dir = (currentTarget.transform.position - transform.position).normalized;
                FireProjectile(dir);
            }
            yield return burstWait;
        }
    }

    private IEnumerator ExecuteRapidFireAttack()
    {
        for (int i = 0; i < 9; i++)
        {
            if (!isAngry) yield break; // Exit if anger ended

            // Only recalculate target every few shots for performance
            if (i % targetUpdateFrequency == 0 || currentTarget == null)
            {
                currentTarget = GetClosestPlayer();
            }
            
            if (currentTarget != null)
            {
                Vector2 dir = (currentTarget.transform.position - transform.position).normalized;
                FireProjectile(dir);
            }
            yield return rapidFireWait;
        }
    }

    private void FireProjectile(Vector2 direction, float scale = 1f)
    {
        if (!shoutProjectilePrefab || !shoutSpawnPoint) return;
        if (!isAngry) return; // Don't fire if not angry

        GameObject projectile = Instantiate(shoutProjectilePrefab, shoutSpawnPoint.position, Quaternion.identity);
        projectile.transform.localScale *= scale;

        if (projectile.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.velocity = direction * projectileSpeed * scale;
        }

        var projectileCollider = projectile.GetComponent<Collider2D>();
        if (projectileCollider != null && npcCollider != null)
        {
            Physics2D.IgnoreCollision(projectileCollider, npcCollider);
        }

        // Set bonus damage for big projectiles
        if (projectile.TryGetComponent<ShoutProjectile>(out var shout))
        {
            shout.damage *= scale; // scale = 2 for big, so double the damage
        }
    }
}