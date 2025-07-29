using UnityEngine;

[RequireComponent(typeof(Animator))]
public class NPCAnimationController : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private string moveParameter = "isMoving";
    [SerializeField] private string AngryParameter = "isAngry";

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

    private bool isAngry = false;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (useRigidbody2D && targetRigidbody == null)
        {
            targetRigidbody = GetComponentInParent<Rigidbody2D>();
        }
    }

    private void OnEnable()
    {
        lastPosition = transform.position;
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

        // Maintain current angry states
        animator.SetBool(AngryParameter, isAngry);
    }

    public bool IsAngry // Property to get/set anger state
    {
        get => isAngry;
        set => SetIsAngry(value);
    }

    public void SetIsAngry(bool state)
    {
        isAngry = state;
        animator.SetBool(AngryParameter, state);
    }
}