using UnityEngine;

public static class CursorPhysicsHelper
{
    public static bool IsCursorOverTilemap(Vector2 position, float radius, LayerMask tilemapLayer)
    {
        return Physics2D.OverlapCircle(position, radius, tilemapLayer);
    }
}

public class NoThrowCursorDetector : MonoBehaviour
{
    public enum InputType { Keyboard, Joystick }
    public InputType inputType;

    [SerializeField] private float detectionRadius = 0.05f;
    [SerializeField] private LayerMask tilemapLayer = default;

    private PlayerInputManager inputManager;
    private PlayerInputManagerP2 inputManagerP2;

    void Start()
    {
        inputManager = GetComponent<PlayerInputManager>();
        inputManagerP2 = GetComponent<PlayerInputManagerP2>();
    }

    void Update()
    {
        Vector2 cursorPos = inputType switch
        {
            InputType.Keyboard => ScreenToWorldPointMouse.Instance.GetMouseWorldPosition(),
            InputType.Joystick => PlayerAimController.Instance.GetCursorPosition(),
            _ => Vector2.zero
        };

        bool isOverNoThrow = Physics2D.OverlapCircle(cursorPos, detectionRadius, tilemapLayer);

        if (inputManager != null)
            inputManager.canThrow = !isOverNoThrow;

        if (inputManagerP2 != null)
            inputManagerP2.canThrow = !isOverNoThrow;
    }
}
