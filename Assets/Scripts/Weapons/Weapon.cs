using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public int durability = 10; // Durability value
    public bool isThrowable = false;
    public bool isFirearm = false;

    public virtual void UseAsMelee()
    {
        durability--;
        CheckDurability();
    }

    public virtual void Shoot()
    {
        // Firearm-specific behavior
    }

    public virtual void Throw(Vector3 direction, float force)
    {
        if (isThrowable)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.isKinematic = false; // Allow physics simulation
                rb.velocity = direction * force;
            }
            durability--;
            CheckDurability();
        }
    }

    protected void CheckDurability()
    {
        if (durability <= 0)
        {
            durability = 0;
            // Make weapon unusable (can still be picked up but not used)
            gameObject.SetActive(false);
        }
    }
}