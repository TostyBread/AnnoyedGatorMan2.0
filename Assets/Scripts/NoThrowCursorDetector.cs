using UnityEngine;

public class NoThrowCursorDetector : MonoBehaviour
{
    public enum InputType { Mouse, Joystick }
    public InputType inputType;

    [SerializeField] private LayerMask wallLayer;

    private PlayerInputManager inputManager;
    private PlayerInputManagerP2 inputManagerP2;

    void Start()
    {
        inputManager = GetComponent<PlayerInputManager>();
        inputManagerP2 = GetComponent<PlayerInputManagerP2>();
    }

    void LateUpdate()
    {
        bool hasHeldItem =
            (inputManager != null && inputManager.HasHeldItem()) ||
            (inputManagerP2 != null && inputManagerP2.HasHeldItem());

        if (hasHeldItem)
        {
            UpdateThrowBlocking();
        }
        else
        {
            if (inputManager != null) inputManager.canThrow = false;
            if (inputManagerP2 != null) inputManagerP2.canThrow = false;
        }
    }

    void UpdateThrowBlocking()
    {
        Vector2 cursorPos = inputType switch
        {
            InputType.Mouse => ScreenToWorldPointMouse.Instance?.GetMouseWorldPosition() ?? Vector2.zero,
            InputType.Joystick => PlayerAimController.Instance?.GetCursorPosition() ?? Vector2.zero,
            _ => Vector2.zero
        };

        if (cursorPos == Vector2.zero) return;

        Vector2 origin = transform.position;
        Vector2 direction = (cursorPos - origin).normalized;
        float distance = Vector2.Distance(origin, cursorPos);

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, wallLayer);
        bool canThrow = true;
        //bool canThrow = hit.collider == null;

        if (inputManager != null) inputManager.canThrow = canThrow;
        if (inputManagerP2 != null) inputManagerP2.canThrow = canThrow;
    }

    void OnDrawGizmosSelected()
    {
        Vector2 origin = transform.position;
        Vector2 target = Application.isPlaying
            ? (inputType == InputType.Mouse
                ? ScreenToWorldPointMouse.Instance?.GetMouseWorldPosition() ?? origin
                : PlayerAimController.Instance?.GetCursorPosition() ?? origin)
            : origin + Vector2.right;

        Vector2 direction = (target - origin).normalized;
        float distance = Vector2.Distance(origin, target);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(origin, direction * Mathf.Min(distance, 20f));
    }
}