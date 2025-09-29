using UnityEngine;

public class PlayerAimController : MonoBehaviour
{
    public static PlayerAimController Instance { get; private set; }

    [Header("UI Cursor")]
    public RectTransform uiCursorRect;
    public Canvas canvas;
    public float cursorSpeed = 1000f;
    public float aimDeadzone = 0.2f;

    [Header("Soft Lock Settings")]
    public LayerMask softLockLayers;
    public float lockOnRadius = 0.5f;

    [Header("Animation Settings")]
    public Animator cursorAnimator;
    public LayerMask interactableLayers;

    private Vector2 cursorScreenPosition;
    private Camera mainCamera;
    private PlayerInputActions inputActions;
    private Transform lockedTarget;
    private PlayerPickupSystemP2 pickupSystem;

    // Animation state tracking
    public enum CursorAnimationState { Normal, Interactable, InRange }
    private CursorAnimationState currentAnimationState = CursorAnimationState.Normal;

    public bool LockOnActive => lockedTarget != null;
    public bool LockOnEnabled { get; set; } = true;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        inputActions = new PlayerInputActions();
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void Start()
    {
        mainCamera = Camera.main;
        cursorScreenPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
        UpdateUICursor();
        
        // Get reference to PlayerPickupSystemP2
        pickupSystem = FindObjectOfType<PlayerPickupSystemP2>();
        if (pickupSystem == null)
        {
            Debug.LogWarning("PlayerPickupSystemP2 not found! In range detection will not work.");
        }

        // Initialize cursor animation
        SetCursorAnimationState(CursorAnimationState.Normal);
    }

    void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return; // Exit early if no camera found
        }

        Vector2 input = inputActions.Player2Controller.Aim.ReadValue<Vector2>();

        if (input.magnitude > aimDeadzone)
        {
            ClearLockOn();
            Vector2 moveDelta = input * cursorSpeed * Time.deltaTime;
            cursorScreenPosition += moveDelta;
            ClampCursorPosition();
            UpdateUICursor();
        }
        else
        {
            TrySoftLock();
        }

        // Update cursor animation based on what we're aiming at
        UpdateCursorAnimation();
    }

    private void TrySoftLock()
    {
        if (!LockOnEnabled || mainCamera == null) return;

        if (lockedTarget != null)
        {
            Vector3 worldToScreen = mainCamera.WorldToScreenPoint(lockedTarget.position);
            cursorScreenPosition = worldToScreen;
            UpdateUICursor();
            return;
        }

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(cursorScreenPosition);
        worldPos.z = 0;
        Collider2D target = Physics2D.OverlapCircle(worldPos, lockOnRadius, softLockLayers);
        if (target == null) return;

        lockedTarget = target.transform;
        Vector3 worldToScreenFinal = mainCamera.WorldToScreenPoint(lockedTarget.position);
        cursorScreenPosition = worldToScreenFinal;
        UpdateUICursor();
    }

    private void UpdateCursorAnimation()
    {
        if (cursorAnimator == null) return;

        // Get cursor world position
        Vector3 cursorWorldPos = GetCursorPosition();
        
        // Check what we're aiming at
        Collider2D hitCollider = Physics2D.OverlapPoint(cursorWorldPos, interactableLayers);
        
        CursorAnimationState newState = CursorAnimationState.Normal;

        if (hitCollider != null)
        {
            // We're aiming at something interactable
            newState = CursorAnimationState.Interactable;

            // Check if we're also in range using PlayerPickupSystemP2
            if (pickupSystem != null && IsInRange(hitCollider))
            {
                newState = CursorAnimationState.InRange;
            }
        }

        // Only update animation if state changed
        if (newState != currentAnimationState)
        {
            SetCursorAnimationState(newState);
        }
    }

    private bool IsInRange(Collider2D target)
    {
        if (pickupSystem == null) return false;

        // Use the same detection method as PlayerPickupSystemP2
        Collider2D[] collidersInRange = Physics2D.OverlapCircleAll(pickupSystem.transform.position, pickupSystem.pickupRadius);
        
        // Check if the target is in the list of colliders within pickup radius
        foreach (Collider2D collider in collidersInRange)
        {
            if (collider == target)
            {
                return true;
            }
        }
        
        return false;
    }

    private void SetCursorAnimationState(CursorAnimationState newState)
    {
        if (cursorAnimator == null) return;

        currentAnimationState = newState;

        // Reset all animation triggers/bools
        cursorAnimator.SetBool("IsInteractable", false);
        cursorAnimator.SetBool("IsInRange", false);

        // Set appropriate animation state
        switch (newState)
        {
            case CursorAnimationState.Normal:
                // Normal state is the default, no additional parameters needed
                break;
            case CursorAnimationState.Interactable:
                cursorAnimator.SetBool("IsInteractable", true);
                break;
            case CursorAnimationState.InRange:
                cursorAnimator.SetBool("IsInteractable", true);
                cursorAnimator.SetBool("IsInRange", true);
                break;
        }
    }

    private void UpdateUICursor()
    {
        if (mainCamera == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                cursorScreenPosition,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
                out Vector2 localPoint))
        {
            uiCursorRect.anchoredPosition = localPoint;
        }
    }

    private void ClampCursorPosition()
    {
        cursorScreenPosition.x = Mathf.Clamp(cursorScreenPosition.x, 0, Screen.width);
        cursorScreenPosition.y = Mathf.Clamp(cursorScreenPosition.y, 0, Screen.height);
    }

    public Vector3 GetCursorPosition()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) 
            {
                Debug.LogWarning("No camera found! Returning Vector3.zero");
                return Vector3.zero;
            }
        }

        Vector3 world = mainCamera.ScreenToWorldPoint(cursorScreenPosition);
        world.z = 0;
        return world;
    }

    public void ClearLockOn()
    {
        lockedTarget = null;
    }

    public void LockOnToTarget(Transform target)
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        if (mainCamera == null) return;

        lockedTarget = target;
        Vector2 screenPos = mainCamera.WorldToScreenPoint(target.position);
        cursorScreenPosition = screenPos;
        UpdateUICursor();
    }

    // Public methods for getting current animation state (useful for debugging or other systems)
    public CursorAnimationState GetCurrentAnimationState() => currentAnimationState;
    
    public bool IsAimingAtInteractable() => currentAnimationState != CursorAnimationState.Normal;
    
    public bool IsInInteractionRange() => currentAnimationState == CursorAnimationState.InRange;
}