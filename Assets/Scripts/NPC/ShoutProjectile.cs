using UnityEngine;

public class ShoutProjectile : MonoBehaviour
{
    public float damage = 1f;
    public float lifetime = 3f;
    public string[] targetTags = new string[] { "Player", "Enemy" }; // Changed to array with default value

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        bool isValidTarget = System.Array.Exists(targetTags, tag => collision.collider.CompareTag(tag));
        
        if (isValidTarget)
        {
            if (collision.collider.TryGetComponent<HealthManager>(out var health))
            {
                health.TryDamage((int)damage, this.gameObject);
            }
            if (collision.collider.CompareTag("Player"))
            {
                var sanity = FindObjectOfType<Sanity>();
                if (sanity != null)
                    sanity.decreaseSanity((int)damage);

                EffectPool.Instance.SpawnEffect("WordExplode", transform.position, Quaternion.identity);
                AudioManager.Instance.PlaySound("OOF", transform.position);
            }
            if (collision.collider.CompareTag("Enemy"))
            {
                EffectPool.Instance.SpawnEffect("WordExplode", transform.position, Quaternion.identity);
            }
        }

        Destroy(gameObject);
    }
}