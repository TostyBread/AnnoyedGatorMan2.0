using UnityEngine;

public class NoThrowCursorDetector : MonoBehaviour
{
    public enum InputType { Mouse, Joystick, P3Joystick } // Added Chee Keat's controller support
    public InputType inputType;

    [SerializeField] private LayerMask wallLayer;
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLog = false;
    [SerializeField] private bool enableDebugGizmos = false;

    private PlayerInputManager inputManager;
    private PlayerInputManagerP2 inputManagerP2;
    private P3Input inputManagerP3; // Added Chee Keat's controller support

    void Start()
    {
        inputManager = GetComponent<PlayerInputManager>();
        inputManagerP2 = GetComponent<PlayerInputManagerP2>();
        inputManagerP3 = GetComponent<P3Input>(); // Added Chee Keat's controller support
    }

    void LateUpdate()
    {
        bool hasHeldItem =
            (inputManager != null && inputManager.HasHeldItem()) ||
            (inputManagerP2 != null && inputManagerP2.HasHeldItem()) ||
            (inputManagerP3 != null && inputManagerP3.HasItemHeld);// Added Chee Keat's controller support

        if (hasHeldItem)
        {
            UpdateThrowBlocking();
        }
        else
        {
            if (inputManager != null) inputManager.canThrow = false;
            if (inputManagerP2 != null) inputManagerP2.canThrow = false;
            if (inputManagerP3 != null) inputManagerP3.canThrow = false; // Added Chee Keat's controller support
        }
    }

    void UpdateThrowBlocking()
    {
        Vector2 cursorPos = inputType switch
        {
            InputType.Mouse => ScreenToWorldPointMouse.Instance?.GetMouseWorldPosition() ?? Vector2.zero,
            InputType.Joystick => PlayerAimController.Instance?.GetCursorPosition() ?? Vector2.zero,
            InputType.P3Joystick => P3Cursor.Instance?.transform.position ?? Vector2.zero, // Added Chee Keat's controller support
            _ => Vector2.zero
        };

        // Enhanced validation for cursor position
        if (cursorPos == Vector2.zero || !IsValidCursorPosition(cursorPos))
        {
            if (enableDebugLog)
                Debug.LogWarning($"[NoThrowCursorDetector] Invalid cursor position: {cursorPos}");
            
            // Allow throwing when cursor position is invalid (common on first play)
            SetCanThrow(true);
            return;
        }

        Vector2 origin = transform.position;
        Vector2 direction = (cursorPos - origin).normalized;
        float distance = Vector2.Distance(origin, cursorPos);

        // Limit raycast distance to prevent hitting distant objects
        float maxRaycastDistance = Mathf.Min(distance, 50f);

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxRaycastDistance, wallLayer);
        
        bool canThrow = hit.collider == null;

        if (enableDebugLog && hit.collider != null)
        {
            Debug.Log($"[NoThrowCursorDetector] Raycast hit: {hit.collider.name} on layer {hit.collider.gameObject.layer}");
        }

        SetCanThrow(canThrow);
    }

    private bool IsValidCursorPosition(Vector2 cursorPos)
    {
        // Check if cursor position is reasonable (not at origin or extremely far)
        float distanceFromPlayer = Vector2.Distance(transform.position, cursorPos);
        return distanceFromPlayer > 0.1f && distanceFromPlayer < 50f;
    }

    private void SetCanThrow(bool canThrow)
    {
        if (inputManager != null) inputManager.canThrow = canThrow;
        if (inputManagerP2 != null) inputManagerP2.canThrow = canThrow;
        if (inputManagerP3 != null) inputManagerP3.canThrow = canThrow; // Added Chee Keat's controller support
    }

    // Set properties for ItemPackage
    public PlayerInputManager InputManager => inputManager;
    public PlayerInputManagerP2 InputManagerP2 => inputManagerP2;
    public P3Input InputManagerP3 => inputManagerP3; // Added Chee Keat's controller support

    void OnDrawGizmosSelected()
    {
        if (!enableDebugGizmos) return;

        Vector2 origin = transform.position;
        Vector2 target = Application.isPlaying
            ? (inputType == InputType.Mouse
                ? ScreenToWorldPointMouse.Instance?.GetMouseWorldPosition() ?? origin
                : PlayerAimController.Instance?.GetCursorPosition() ?? origin)
            : origin + Vector2.right;

        Vector2 direction = (target - origin).normalized;
        float distance = Vector2.Distance(origin, target);
        float maxRaycastDistance = Mathf.Min(distance, 50f);

        // Draw the raycast line
        Gizmos.color = Color.red;
        Gizmos.DrawRay(origin, direction * maxRaycastDistance);

        // Draw cursor position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(target, 0.2f);

        // Draw raycast hit point if there is one
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxRaycastDistance, wallLayer);
        if (hit.collider != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(hit.point, 0.3f);
        }
    }
}