using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float lifetime = 5f; // How long the projectile lives before being disabled

    private float elapsedTime = 0f;

    void Update()
    {
        elapsedTime += Time.deltaTime;

        // Disable or destroy the projectile after its lifetime expires
        if (elapsedTime >= lifetime)
        {
            DestroyBullet();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        DestroyBullet();
    }

    private void DestroyBullet()
    {
        Destroy(gameObject);
    }

}