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
    private GameObject target;
    private Vector3 angerStartPosition;
    [SerializeField] private NPCAnimationController npcAnimator; // Reference NPC's Animator

    public bool IsAngry => isAngry;

    void Awake()
    {
        // grab npcAnimator reference and set accordingly
        if (npcAnimator == null) npcAnimator = GetComponent<NPCAnimationController>();
        npcAnimator.SetIsAngry(false);

        npc = GetComponent<NPCBehavior>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        npcCollider = GetComponent<Collider2D>();
        target = GameObject.FindGameObjectWithTag("Player");

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void TriggerAngerMode(GameObject source)
    {
        if (isAngry) return;
        if (npc.hasAcceptedPlate) return; // Don't trigger anger if the NPC has accepted a plate

        isAngry = true;
        npcAnimator.SetIsAngry(true); // When these condition are true, the NPC will use Angry from this point onward
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
        while (isAngry && target != null)
        {
            if (spriteRenderer != null)
            {
                for (int i = 0; i < blinkCount; i++)
                {
                    spriteRenderer.color = blinkColor;
                    yield return new WaitForSeconds(blinkDuration);
                    spriteRenderer.color = originalColor;
                    yield return new WaitForSeconds(blinkDuration);
                }

                // Flip direction to face player
                if (target.transform.position.x < transform.position.x)
                    spriteRenderer.flipX = true;
                else
                    spriteRenderer.flipX = false;
            }
            AudioManager.Instance.PlaySound("Swear", transform.position); // NPC swearing

            Vector2 dir = (target.transform.position - transform.position).normalized;
            switch (shoutMode)
            {
                case ShoutMode.Single:
                    FireProjectile(dir);
                    break;
                case ShoutMode.Burst:
                    for (int i = 0; i < 3; i++)
                    {
                        dir = (target.transform.position - transform.position).normalized;
                        FireProjectile(dir);
                        yield return new WaitForSeconds(0.3f);
                    }
                    break;
                case ShoutMode.RapidFire:
                    for (int i = 0; i < 9; i++)
                    {
                        dir = (target.transform.position - transform.position).normalized;
                        FireProjectile(dir);
                        yield return new WaitForSeconds(0.2f);
                    }
                    break;
                case ShoutMode.BigProjectile:
                    FireProjectile(dir, 1.5f);
                    break;
            }
            yield return new WaitForSeconds(shoutCooldown);
        }
    }

    private IEnumerator CheckForPushRoutine()
    {
        float checkInterval = 3f;
        float moveThreshold = 0.5f;

        while (isAngry)
        {
            yield return new WaitForSeconds(checkInterval);
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