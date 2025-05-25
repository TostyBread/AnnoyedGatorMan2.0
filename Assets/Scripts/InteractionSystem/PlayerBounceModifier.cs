using System.Diagnostics.Tracing;
using TMPro;
using UnityEditor.U2D.Aseprite;
using UnityEditor.UIElements;
using UnityEngine;

public class PlayerBounceModifier : MonoBehaviour
{
    [Header("Bounciness Settings")]
    public float reducedBounceFactor = 0.1f; // Reduced bounce factor when colliding with the player
    public float normalBounceFactor = 1f;   // Normal bounce factor for other collisions
    public float playerBounceDamping = 0.5f; // Damping factor to reduce velocity upon player collision

    [Header("References")]
    public PhysicsMaterial2D bounceMaterial; // The Physics Material 2D assigned to the item

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        bool isPlayerCollision = false;

        // Removed List of Player collider, because its fucking dumb of you to implement
        if (collision.collider.CompareTag("Player"))
        {
            isPlayerCollision = true;
        }

        if (isPlayerCollision)
        {
            HandlePlayerCollision(collision);
        }
        else
        {
            ResetBounceMaterial();
        }
    }

    private void HandlePlayerCollision(Collision2D collision)
    {
        // Temporarily reduce the bounciness for player collision
        if (bounceMaterial != null)
        {
            bounceMaterial.bounciness = reducedBounceFactor;
        }

        // Apply damping to reduce the velocity on collision
        if (rb != null)
        {
            rb.velocity *= playerBounceDamping; // Reduce the velocity for less bounce effect
        }
        //Debug.Log("Collided with player, reduced bounciness applied.");
    }

    private void ResetBounceMaterial()
    {
        // Restore the normal bounciness for non-player collisions
        if (bounceMaterial != null)
        {
            bounceMaterial.bounciness = normalBounceFactor;
        }
    }
}