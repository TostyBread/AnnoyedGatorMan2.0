using System.Collections;
using UnityEngine;

/// <summary>
/// Simple hitbox that damages relevant targets and deactivates after the frame.
/// Reads aimForFood from parent each frame to stay in sync.
/// </summary>
public class HitBox : MonoBehaviour
{
    public float damage = 10;
    public bool aimForFood;

    private EnemyMovement parentEM;

    private void Awake()
    {
        parentEM = GetComponentInParent<EnemyMovement>();
    }

    private void Start()
    {
        gameObject.SetActive(false);
        if (parentEM != null) aimForFood = parentEM.aimForFood;
    }

    private void Update()
    {
        if (parentEM != null) aimForFood = parentEM.aimForFood;
    }

    private void TryDamageCollider(GameObject target)
    {
        if (target == null) return;

        // Try get from children first
        var hm = target.GetComponentInChildren<HealthManager>();

        // If not found, try get from the target itself
        if (hm == null)
            hm = target.GetComponent<HealthManager>();

        // If we found a HealthManager anywhere, deal damage
        if (hm != null)
        {
            hm.TryDamage(damage, this.gameObject);
        }
        else
        {
            Debug.LogWarning($"{target.name} has no HealthManager component.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!gameObject.activeInHierarchy) return;

        if (!aimForFood && collision.CompareTag("Player"))
        {
            TryDamageCollider(collision.gameObject);
        }
        else if (aimForFood && (collision.CompareTag("FoodBig") || collision.CompareTag("FoodSmall")))
        {
            TryDamageCollider(collision.gameObject);
        }

        // Deactivate at end of frame so other overlapping colliders can also register this frame.
        StartCoroutine(DisableAtEndOfFrame());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!gameObject.activeInHierarchy) return;

        if (!aimForFood && collision.gameObject.CompareTag("Player"))
        {
            TryDamageCollider(collision.gameObject);
        }
        else if (aimForFood && (collision.gameObject.CompareTag("FoodBig") || collision.gameObject.CompareTag("FoodSmall")))
        {
            TryDamageCollider(collision.gameObject);
        }

        StartCoroutine(DisableAtEndOfFrame());
    }

    private IEnumerator DisableAtEndOfFrame()
    {
        // Wait one frame so multiple overlaps in the same frame can cause damage.
        yield return null;
        gameObject.SetActive(false);
    }
}