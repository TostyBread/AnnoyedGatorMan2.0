using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using static FirearmController;

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

    private HashSet<GameObject> objectsInFire = new HashSet<GameObject>();
    private Coroutine heatCoroutine;
    private Rigidbody2D rb;
    private float originalHeatAmount;

    private Sanity sanity;

    //Get parent to ignore self damage & self collision 
    public GameObject playerInParent;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sanity = GameObject.FindGameObjectWithTag("Sanity").GetComponent<Sanity>();
        originalHeatAmount = heatAmount;

        playerInParent = GetPlayerInParent(transform).gameObject; // Find the top parent to ignore self damage and collision
    }

    public Transform GetPlayerInParent(Transform current)
    {
        while (current.parent != null && !current.CompareTag("Player")) current = current.parent;
        return current;
    }

    public void SetOwner(GameObject newOwner)
    {
        playerInParent = newOwner;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ( damageAmount == 0 || isFireSource || isColdSource || isStunSource || rb == null || sanity == null || rb.velocity.magnitude < minVelocityToDamage)
        {
            return;
        }

        if (collision.collider.TryGetComponent(out ItemSystem item)) // For damaging Item
        {
            if (damageAmount != 0) // if no damage given, dont execute any debris effect
            {
                item.ApplyCollisionEffect(gameObject);
                if (playHitSound) PlayHitSound(damageType);
                DebrisManager.Instance.PlayDebrisEffect("DebrisPrefab", collision.contacts[0].point, damageType);
            }
        }
        else
        {
            if (playHitSound) AudioManager.Instance.PlaySound(damageType == ItemSystem.DamageType.Shot ? "Ricochet" : "GunHit", 1.0f, transform.position);
            DebrisManager.Instance.PlayDebrisEffect("DebrisPrefab", collision.contacts[0].point, "SparkSpurt");
        }

        if (collision.gameObject.TryGetComponent(out HealthManager health)) // For damaging health
        {
            if (playerInParent != null)
            {
                //Ignore self damage & collision with parent object
                if (collision.gameObject == playerInParent)
                {
                    Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>());
                }

                if (collision.gameObject != playerInParent)
                {
                    health.TryDamage(damageAmount);

                    if (collision.gameObject.CompareTag("Player"))
                    {
                        sanity.decreaseSanity(damageAmount);
                    }
                }
            }
        }

        if (collision.collider.TryGetComponent(out TrashBag trashBag)) // For damaging trash bag health
        {
            trashBag.TryDamaging(damageAmount);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isFireSource || isColdSource || isStunSource) return;

        if (other.TryGetComponent(out ItemSystem item) && objectsInFire.Add(other.gameObject))
        {
            if (heatCoroutine == null)
            {
                heatCoroutine = StartCoroutine(ApplyHeatOverTime());
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
            // Create a temporary copy to safely iterate
            var objectsSnapshot = new List<GameObject>(objectsInFire);

            foreach (var obj in objectsSnapshot)
            {
                if (obj != null && obj.TryGetComponent(out ItemSystem item))
                {
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
        AudioManager.Instance.PlaySound(soundName, 1.0f, transform.position);
    }

    public void ResetHeat()
    {
        heatAmount = originalHeatAmount;
    }
}