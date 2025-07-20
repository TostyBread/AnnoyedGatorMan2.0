using UnityEngine;

public class ShoutProjectile : MonoBehaviour
{
    public float damage = 1f;
    public float lifetime = 3f;
    public string targetTag = "Player";

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(targetTag))
        {
            if (collision.collider.TryGetComponent<HealthManager>(out var health))
            {
                health.TryDamage((int)damage);
            }
            if (collision.collider.CompareTag("Player"))
            {
                var sanity = FindObjectOfType<Sanity>();
                if (sanity != null)
                    sanity.decreaseSanity((int)damage);
            }
        }

        // EffectPool.Instance.SpawnEffect("Explosion", transform.position, Quaternion.identity); // VFX effect spawned
        // AudioManager.Instance.PlaySound("WordImpact", transform.position); // Impact sound effect

        Destroy(gameObject);
    }
}