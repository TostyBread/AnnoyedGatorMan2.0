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
    public float minVelocityToDamage = 0f;

    private HashSet<GameObject> objectsInFire = new HashSet<GameObject>();
    private Coroutine heatCoroutine;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isFireSource || rb == null || rb.velocity.magnitude < minVelocityToDamage)
        {
            return;
        }

        if (collision.collider.TryGetComponent(out ItemSystem item))
        {
            item.ApplyCollisionEffect(gameObject);
            PlayHitSound(damageType);
        }
        else
        {
            AudioManager.Instance.PlaySound(damageType == ItemSystem.DamageType.Shot ? "Ricochet" : "GunHit", 1.0f, transform.position);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isFireSource) return;

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
        if (!isFireSource || !objectsInFire.Remove(other.gameObject) || objectsInFire.Count > 0) return;

        StopCoroutine(heatCoroutine);
        heatCoroutine = null;
    }

    private IEnumerator ApplyHeatOverTime()
    {
        while (objectsInFire.Count > 0)
        {
            foreach (var obj in objectsInFire)
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
        string soundName = type switch
        {
            ItemSystem.DamageType.Bash => "BashHit",
            ItemSystem.DamageType.Cut => "KnifeHit",
            ItemSystem.DamageType.Shot => "GunHit",
            _ => "Non"
        };
        AudioManager.Instance.PlaySound(soundName, 1.0f, transform.position);
    }
}