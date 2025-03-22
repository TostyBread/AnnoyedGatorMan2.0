using UnityEngine;

public class RandomMovement2D : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of movement

    private Rigidbody2D rb; // Reference to the Rigidbody2D component
    private Transform target; // The target to move toward

    public bool IsMoving => rb.velocity.sqrMagnitude > 0.01f; // Public property for animation script

    private void Start()
    {
        // Find the target with the "Player" tag
        target = GameObject.FindGameObjectWithTag("Player").transform;

        // Get the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (target == null) return;

        // Calculate the direction to the target
        Vector2 direction = (target.position - transform.position).normalized;

        // Move toward the target
        rb.velocity = direction * moveSpeed;
    }
}