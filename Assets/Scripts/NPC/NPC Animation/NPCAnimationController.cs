using UnityEngine;

/// <summary>
/// Flexible 2D NPC animation controller. Supports idle and moving states.
/// Works with both Rigidbody2D or manual movement.
/// </summary>
[RequireComponent(typeof(Animator))]
public class NPCAnimationController : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private string moveParameter = "isMoving";

    [Header("Movement Settings")]
    [SerializeField] private bool useRigidbody2D = true;
    [SerializeField] private float movementThreshold = 0.01f;
    [SerializeField] private bool flipSpriteOnX = true;
    [Tooltip("Leave empty to auto-detect on parent")]
    [SerializeField] private Rigidbody2D targetRigidbody;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 lastPosition;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (useRigidbody2D)
        {
            if (targetRigidbody == null)
            {
                targetRigidbody = GetComponentInParent<Rigidbody2D>();
            }
        }
    }

    private void Update()
    {
        Vector2 velocity = Vector2.zero;

        if (useRigidbody2D && targetRigidbody != null)
        {
            velocity = targetRigidbody.velocity;
        }
        else
        {
            velocity = (Vector2)transform.position - lastPosition;
            lastPosition = transform.position;
        }

        bool isMoving = velocity.magnitude > movementThreshold;
        animator.SetBool(moveParameter, isMoving);

        if (flipSpriteOnX && velocity.x != 0 && spriteRenderer != null)
        {
            spriteRenderer.flipX = velocity.x < 0;
        }
    }

    private void OnEnable()
    {
        if (!useRigidbody2D)
            lastPosition = transform.position;
    }
}