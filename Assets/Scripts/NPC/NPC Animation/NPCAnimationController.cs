using UnityEngine;

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
    private Vector2 smoothedVelocity;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (useRigidbody2D && targetRigidbody == null)
        {
            targetRigidbody = GetComponentInParent<Rigidbody2D>();
        }
    }

    private void FixedUpdate()
    {
        Vector2 velocity = Vector2.zero;

        if (useRigidbody2D && targetRigidbody != null)
        {
            velocity = ((Vector2)transform.position - lastPosition) / Time.fixedDeltaTime;
        }
        else
        {
            velocity = (Vector2)transform.position - lastPosition;
        }

        lastPosition = transform.position;

        smoothedVelocity = Vector2.Lerp(smoothedVelocity, velocity, 0.5f);

        bool isMoving = smoothedVelocity.magnitude > movementThreshold;
        animator.SetBool(moveParameter, isMoving);

        if (flipSpriteOnX && Mathf.Abs(smoothedVelocity.x) > 0.01f && spriteRenderer != null)
        {
            spriteRenderer.flipX = smoothedVelocity.x < 0;
        }
    }

    private void OnEnable()
    {
        lastPosition = transform.position;
    }
}