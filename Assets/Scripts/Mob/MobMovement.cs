using UnityEngine;

public class RandomMovement2D : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of movement

    private Rigidbody2D rb; // Reference to the Rigidbody2D component
    private Transform target; // The target to move toward
    private Vector2 direction;

    public bool IsMoving => rb.velocity.sqrMagnitude > 0.01f; // Public property for animation script

    private MobSpawner mobSpawner;

    private void Start()
    {
        // Find the target with the "Player" tag
        target = GameObject.FindGameObjectWithTag("Player").transform;
        mobSpawner = GameObject.FindGameObjectWithTag("Spawner"). GetComponent<MobSpawner>();

        // Get the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (target == null || !mobSpawner) return;

        if (mobSpawner.timer.RemainTime > 0)
        {
            direction = (mobSpawner.spawnPos.transform.position - transform.position).normalized;
        }
        else 
        {
            // Calculate the direction to the target
            direction = (target.position - transform.position).normalized;            
        }

        // Move toward the target
        rb.velocity = direction * moveSpeed;
    }
}