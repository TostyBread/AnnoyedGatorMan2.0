using Unity.VisualScripting;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public bool canMove { get; set; } = true;

    private Rigidbody2D rb;
    private Vector2 movement;

    public bool IsMoving => movement.sqrMagnitude > 0.01f; // Public property for animation script

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        Move();
    }

    public void SetMovement(Vector2 newMovement)
    {
        movement = canMove ? newMovement : Vector2.zero;
    }

    private void Move()
    {
        if (rb.bodyType == RigidbodyType2D.Static) return; // (DEFENSIVE CODE) ignore incase player is in static state

        rb.velocity = movement * moveSpeed;
    }
}