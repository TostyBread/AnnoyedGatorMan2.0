using UnityEngine;

public class NoThrowCursorDetector : MonoBehaviour
{
    public enum InputType { Mouse, Joystick, P3Joystick }
    public InputType inputType;

    [SerializeField] private LayerMask wallLayer;

    [Header("Raycast Settings")]
    [SerializeField] private float raycastOffsetDistance = 0.5f; // Why: avoid self-collision

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = false;
    [SerializeField] private bool enableDebugGizmos = false;

    private PlayerInputManager inputManager;
    private PlayerInputManagerP2 inputManagerP2;
    private P3Input inputManagerP3;

    void Start()
    {
        inputManager = GetComponent<PlayerInputManager>();
        inputManagerP2 = GetComponent<PlayerInputManagerP2>();
        inputManagerP3 = GetComponent<P3Input>();
    }

    void LateUpdate()
    {
        bool hasHeldItem =
            (inputManager != null && inputManager.HasHeldItem()) ||
            (inputManagerP2 != null && inputManagerP2.HasHeldItem()) ||
            (inputManagerP3 != null && inputManagerP3.HasItemHeld);

        if (hasHeldItem)
        {
            UpdateThrowBlocking();
        }
        else
        {
            SetCanThrow(false);
        }
    }

    void UpdateThrowBlocking()
    {
        Vector2 cursorPos = inputType switch
        {
            InputType.Mouse => ScreenToWorldPointMouse.Instance?.GetMouseWorldPosition() ?? Vector2.zero,
            InputType.Joystick => PlayerAimController.Instance?.GetCursorPosition() ?? Vector2.zero,
            InputType.P3Joystick => P3Cursor.Instance?.transform.position ?? Vector2.zero,
            _ => Vector2.zero
        };

        if (!IsValidCursorPosition(cursorPos))
        {
            if (enableDebugLog)
                Debug.LogWarning($"[NoThrowCursorDetector] Invalid cursor position: {cursorPos}");

            SetCanThrow(true);
            return;
        }

        Vector2 playerPos = transform.position;
        Vector2 direction = (cursorPos - playerPos).normalized;

        // offset the raycast origin slightly in front of player
        float offset = Mathf.Min(raycastOffsetDistance, Vector2.Distance(playerPos, cursorPos) - 0.05f);
        Vector2 origin = playerPos + (direction * Mathf.Max(1.8f, offset));

        float distance = Vector2.Distance(origin, cursorPos);
        float maxRaycastDistance = Mathf.Min(distance, 50f);

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxRaycastDistance, wallLayer);
        bool canThrow = hit.collider == null;

        if (enableDebugLog && hit.collider != null)
        {
            Debug.Log($"[NoThrowCursorDetector] Raycast hit: {hit.collider.name}");
        }

        SetCanThrow(canThrow);
    }

    private bool IsValidCursorPosition(Vector2 cursorPos)
    {
        float d = Vector2.Distance(transform.position, cursorPos);
        return d > 0.1f && d < 50f;
    }

    // Set properties for ItemPackage
    public PlayerInputManager InputManager => inputManager;
    public PlayerInputManagerP2 InputManagerP2 => inputManagerP2;
    public P3Input InputManagerP3 => inputManagerP3; // Added Chee Keat's controller support

    private void SetCanThrow(bool state)
    {
        if (inputManager != null) inputManager.canThrow = state;
        if (inputManagerP2 != null) inputManagerP2.canThrow = state;
        if (inputManagerP3 != null) inputManagerP3.canThrow = state;
    }

    void OnDrawGizmosSelected()
    {
        if (!enableDebugGizmos) return;

        if (!Application.isPlaying) return;

        Vector2 playerPos = transform.position;
        Vector2 cursorPos = PlayerAimController.Instance?.GetCursorPosition() ?? playerPos;
        Vector2 direction = (cursorPos - playerPos).normalized;

        float offset = Mathf.Min(raycastOffsetDistance, Vector2.Distance(playerPos, cursorPos) - 0.05f);
        Vector2 origin = playerPos + (direction * Mathf.Max(0f, offset));

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(origin, 0.15f);

        float distance = Vector2.Distance(origin, cursorPos);
        float rayDist = Mathf.Min(distance, 50f);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(origin, direction * rayDist);
    }
}
