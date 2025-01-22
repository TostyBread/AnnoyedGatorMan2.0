using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float lifetime = 5f; // How long the projectile lives before being disabled
    public bool destroyOnCollision = true; // Whether the projectile should be destroyed upon collision

    private float elapsedTime = 0f;

    void Update()
    {
        elapsedTime += Time.deltaTime;

        // Disable or destroy the projectile after its lifetime expires
        if (elapsedTime >= lifetime)
        {
            DisableOrDestroy();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (destroyOnCollision)
        {
            DisableOrDestroy();
        }
    }

    private void DisableOrDestroy()
    {
        // Implement pooling if needed
        Destroy(gameObject);
    }
}