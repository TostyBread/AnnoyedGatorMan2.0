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
    public LayerMask projectileIgnoreLayers;

    private int hitCount = 0;
    private bool isAngry = false;
    private Coroutine shoutRoutine;
    private ShoutMode shoutMode;
    private NPCBehavior npc;
    private bool hasShoutModeSet = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Collider2D npcCollider;

    public bool IsAngry => isAngry;

    void Awake()
    {
        npc = GetComponent<NPCBehavior>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        npcCollider = GetComponent<Collider2D>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void TriggerAngerMode(GameObject source)
    {
        if (isAngry) return;

        isAngry = true;

        npc.rb.velocity = Vector2.zero;
        npc.rb.isKinematic = true;
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

        npc.rb.isKinematic = false;
        isAngry = false;
        hasShoutModeSet = false;

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    private IEnumerator ShoutingLoop()
    {
        GameObject target = GameObject.FindGameObjectWithTag("Player");
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
            }

            Vector2 dir = (target.transform.position - transform.position).normalized;
            switch (shoutMode)
            {
                case ShoutMode.Single:
                    FireProjectile(dir);
                    break;
                case ShoutMode.Burst:
                    for (int i = 0; i < 3; i++)
                    {
                        FireProjectile(dir);
                        yield return new WaitForSeconds(0.2f);
                    }
                    break;
                case ShoutMode.RapidFire:
                    for (int i = 0; i < 5; i++)
                    {
                        FireProjectile(dir);
                        yield return new WaitForSeconds(0.1f);
                    }
                    break;
                case ShoutMode.BigProjectile:
                    FireProjectile(dir, 2f);
                    break;
            }
            yield return new WaitForSeconds(shoutCooldown);
        }
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
        if (projectileCollider != null)
        {
            // Apply projectile layer
            projectile.layer = LayerMask.NameToLayer("EnemyWORDS");

            if (npcCollider != null)
            {
                Physics2D.IgnoreCollision(projectileCollider, npcCollider);
            }
        }
    }
}