using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSource : MonoBehaviour
{
    public ItemSystem.DamageType damageType;
    public int damageAmount;
    public float heatAmount;
    public float heatCooldown = 1.0f;
    public bool isFireSource = false;
    public bool isColdSource = false;
    public bool isStunSource = false;
    public float minVelocityToDamage = 0f;
    public bool playHitSound = true;

    [Header("References")]
    //Get parent to ignore self damage & self collision (must assign for fist) 
    public GameObject owner;
    private Collider2D ownerCollider;

    public bool isHeatAdjusted = false; // Used for adjusting heat amount by weather manager

    private HashSet<GameObject> objectsInFire = new HashSet<GameObject>();
    private Coroutine heatCoroutine;
    private Rigidbody2D rb;
    private float originalHeatAmount;
    //private Sanity sanity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        //sanity = GameObject.FindGameObjectWithTag("Sanity").GetComponent<Sanity>();
        originalHeatAmount = heatAmount;

        if (owner != null) ownerCollider = owner.GetComponent<Collider2D>();
    }
    public void SetOwner(GameObject newOwner)
    {
        // Re-enable collisions with old owner first
        if (ownerCollider != null)
        {
            Physics2D.IgnoreCollision(ownerCollider, GetComponent<Collider2D>(), false);
        }

        owner = newOwner;

        if (owner != null)
        {
            ownerCollider = owner.GetComponent<Collider2D>();
            if (ownerCollider != null)
            {
                Physics2D.IgnoreCollision(ownerCollider, GetComponent<Collider2D>(), true);
            }
        }
        else
        {
            ownerCollider = null;
        }
    }

    public void ClearOwner()
    {
        if (ownerCollider != null)
        {
            Physics2D.IgnoreCollision(ownerCollider, GetComponent<Collider2D>(), false);
            ownerCollider = null;
        }
        owner = null;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (damageAmount == 0 || isFireSource || isColdSource || isStunSource || rb == null || rb.velocity.magnitude < minVelocityToDamage)
            return;

        GameObject target = collision.gameObject;

        // Handle ItemSystem but cancel velocity to prevent push
        if (target.TryGetComponent(out ItemSystem item))
        {
            // Prevent item from being pushed
            if (target.TryGetComponent<Rigidbody2D>(out var itemRb))
            {
                itemRb.velocity = Vector2.zero;
                itemRb.angularVelocity = 0f;
            }

            if (damageAmount != 0)
            {
                item.ApplyCollisionEffect(gameObject);
                if (playHitSound) PlayHitSound(damageType);
                DebrisManager.Instance.PlayDebrisEffect("DebrisPrefab", collision.contacts[0].point, damageType);
            }
        }

        // Allow physical interaction for HealthManager
        if (target.TryGetComponent(out HealthManager health) && target != owner)
        {
            health.TryDamage(damageAmount, this.gameObject);
            //if (target.CompareTag("Player"))
            //{
            //    sanity.decreaseSanity(damageAmount);
            //}
        }

        // Allow physical interaction for TrashBag
        if (target.TryGetComponent(out TrashBag trashBag))
        {
            trashBag.TryDamaging(damageAmount);
        }

        // Allow physical interaction for gun
        if (target.TryGetComponent(out FirearmCrazy gun))
        {
            gun.AddHeat(heatAmount);
            if (playHitSound) PlayHitSound(damageType);
            DebrisManager.Instance.PlayDebrisEffect("DebrisPrefab", collision.contacts[0].point, damageType);
        }

        // Visual feedback for anything else
        if (!target.TryGetComponent<ItemSystem>(out _)) // Skip duplicate debris if already handled above
        {
            if (playHitSound)
                AudioManager.Instance.PlaySound(damageType == ItemSystem.DamageType.Shot ? "Ricochet" : "GunHit", transform.position);

            DebrisManager.Instance.PlayDebrisEffect("DebrisPrefab", collision.contacts[0].point, "SparkSpurt");
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isFireSource || isColdSource || isStunSource) return;

        GameObject target = other.gameObject;
        if (target == null) return;

        // For ItemSystem (existing logic)
        if (other.TryGetComponent(out ItemSystem item))
        {
            if (!objectsInFire.Contains(target))
            {
                objectsInFire.Add(target);

                if (heatCoroutine == null)
                {
                    heatCoroutine = StartCoroutine(ApplyHeatOverTime());
                }
            }
        }

        // For FirearmCrazy (new logic)
        if (other.TryGetComponent(out FirearmCrazy gun))
        {
            if (!objectsInFire.Contains(target))
            {
                objectsInFire.Add(target);

                if (heatCoroutine == null)
                {
                    heatCoroutine = StartCoroutine(ApplyHeatOverTime());
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isFireSource || isColdSource || isStunSource) return;

        if (objectsInFire.Remove(other.gameObject) && objectsInFire.Count == 0)
        {
            if (heatCoroutine != null)
            {
                StopCoroutine(heatCoroutine);
                heatCoroutine = null;
            }
        }
    }

    private IEnumerator ApplyHeatOverTime()
    {
        while (objectsInFire.Count > 0)
        {
            objectsInFire.RemoveWhere(obj => obj == null);

            var snapshot = new List<GameObject>(objectsInFire);

            foreach (var obj in snapshot)
            {
                if (obj == null) continue;

                // Skip if FirearmCrazy exists but collider is disabled
                var gun = obj.GetComponent<FirearmCrazy>();
                if (gun != null)
                {
                    if (gun.firearmCollider == null || !gun.firearmCollider.enabled) continue;
                    gun.AddHeat(heatAmount);
                }

                // ItemSystem logic - check if collider is disabled (item picked up by plate)
                var item = obj.GetComponent<ItemSystem>();
                if (item != null)
                {
                    var itemCollider = obj.GetComponent<Collider2D>();
                    if (itemCollider != null && !itemCollider.enabled) continue; // Skip if collider disabled
                    item.ApplyCollisionEffect(gameObject);
                }
            }
            yield return new WaitForSeconds(heatCooldown);
        }

        heatCoroutine = null;
    }

    private void PlayHitSound(ItemSystem.DamageType type)
    {
        if (AudioManager.Instance == null) return;

        string soundName = type switch
        {
            ItemSystem.DamageType.Bash => "BashHit",
            ItemSystem.DamageType.Cut => "KnifeHit",
            ItemSystem.DamageType.Shot => "GunHit",
            _ => "Non"
        };
        AudioManager.Instance.PlaySound(soundName, transform.position);
    }

    public void ResetHeat()
    {
        heatAmount = originalHeatAmount;
    }

    public void RemoveFromFireObjects(GameObject obj)
    {
        if (obj == null) return;
        objectsInFire.Remove(obj);
    }
}