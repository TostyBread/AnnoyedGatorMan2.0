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
    
    [Header("Performance Settings")]
    [Tooltip("How often to refresh the player list (in seconds)")]
    public float playerListRefreshRate = 2f;
    [Tooltip("How often to recalculate closest player during rapid fire (every N shots)")]
    public int targetUpdateFrequency = 3;

    private int hitCount = 0;
    private bool isAngry = false;
    private Coroutine shoutRoutine;
    private Coroutine returnRoutine;
    private ShoutMode shoutMode;
    private NPCBehavior npc;
    private bool hasShoutModeSet = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Collider2D npcCollider;
    private GameObject[] allPlayers;
    private Vector3 angerStartPosition;
    private NPCAnimationController npcAnim; // Cache the component

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

    void Awake()
    {
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
        angerStartPosition = transform.position;

        npc.rb.velocity = Vector2.zero;
        npc.rb.bodyType = RigidbodyType2D.Dynamic;
        if (npcCollider != null)
            npcCollider.enabled = true;

        if (!hasShoutModeSet)
        {
            shoutMode = (ShoutMode)Random.Range(0, System.Enum.GetValues(typeof(ShoutMode)).Length);
            hasShoutModeSet = true;
        }

        if (shoutRoutine != null)
            StopCoroutine(shoutRoutine);
        shoutRoutine = StartCoroutine(ShoutingLoop());

        if (returnRoutine != null)
            StopCoroutine(returnRoutine);
        returnRoutine = StartCoroutine(CheckForPushRoutine());
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
            if (hitCount >= hitsToTriggerEscape)
            {
                ExitAngerMode();
                npc.ForceEscape();
            }
        }
    }

    private void ExitAngerMode()
    {
        if (shoutRoutine != null)
        {
            StopCoroutine(shoutRoutine);
            shoutRoutine = null;
        }

        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        npc.rb.bodyType = RigidbodyType2D.Dynamic;
        isAngry = false;
        hasShoutModeSet = false;

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
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
                    spriteRenderer.color = blinkColor;
                    yield return blinkWait;
                    spriteRenderer.color = originalColor;
                    yield return blinkWait;
                }

                // Flip direction to face the closest player
                if (currentTarget.transform.position.x < transform.position.x)
                    spriteRenderer.flipX = true;
                else
                    spriteRenderer.flipX = false;
            }
            AudioManager.Instance.PlaySound("Swear", transform.position); // NPC swearing

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
                    FireProjectile(dir, 1.5f);
                    break;
            }
            yield return shoutCooldownWait;
        }
    }

    private IEnumerator ExecuteBurstAttack()
    {
        for (int i = 0; i < 3; i++)
        {
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

    private IEnumerator CheckForPushRoutine()
    {
        float checkInterval = 3f;
        float moveThreshold = 0.5f;
        WaitForSeconds checkWait = new WaitForSeconds(checkInterval);

        while (isAngry)
        {
            yield return checkWait;
            if (Vector3.Distance(transform.position, angerStartPosition) > moveThreshold)
            {
                StartCoroutine(ReturnToAngerPosition());
            }
        }
    }

    private IEnumerator ReturnToAngerPosition()
    {
        float elapsed = 0f;
        float duration = 0.5f;
        Vector3 startPos = transform.position;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, angerStartPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = angerStartPosition;
    }

    private void FireProjectile(Vector2 direction, float scale = 1f)
    {
        if (!shoutProjectilePrefab || !shoutSpawnPoint) return;

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