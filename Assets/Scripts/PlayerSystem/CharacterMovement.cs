using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;

    private Vector2 movement;

    public bool IsMoving => movement.sqrMagnitude > 0.01f; // Public property for animation script

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Move();
    }

    public void SetMovement(Vector2 newMovement)
    {
        movement = newMovement;
    }

    private void Move()
    {
        rb.velocity = movement * moveSpeed;
    }
}