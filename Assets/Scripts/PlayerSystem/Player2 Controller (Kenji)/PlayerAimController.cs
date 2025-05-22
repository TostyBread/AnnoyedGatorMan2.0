using UnityEngine;

public class PlayerAimController : MonoBehaviour
{
    public static PlayerAimController Instance { get; private set; }

    [Header("Joystick Axis Settings")]
    public string joystickHorizontalAxis = "P2_RJoystick_Horizontal";
    public string joystickVerticalAxis = "P2_RJoystick_Vertical";

    [Header("Cursor Settings")]
    public Transform virtualCursorTransform;
    public float cursorSpeed = 5f;
    public float aimDeadzone = 0.2f;

    private Vector3 cursorPosition;
    private Camera mainCamera;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        mainCamera = Camera.main;

        if (virtualCursorTransform != null)
        {
            // Initialize cursor at center of screen in world space
            Vector3 centerViewport = new Vector3(0.5f, 0.5f, mainCamera.nearClipPlane);
            Vector3 centerWorld = mainCamera.ViewportToWorldPoint(centerViewport);
            centerWorld.z = 0f;
            cursorPosition = centerWorld;
            virtualCursorTransform.position = cursorPosition;
        }
    }

    void Update()
    {
        float x = Input.GetAxis(joystickHorizontalAxis);
        float y = Input.GetAxis(joystickVerticalAxis);
        Vector2 input = new Vector2(x, y);

        if (input.magnitude > aimDeadzone)
        {
            Vector3 moveDelta = new Vector3(input.x, input.y, 0f) * cursorSpeed * Time.deltaTime;
            cursorPosition += moveDelta;

            ClampCursorPosition();

            virtualCursorTransform.position = cursorPosition;
        }
    }

    private void ClampCursorPosition()
    {
        Vector3 minScreenBounds = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        Vector3 maxScreenBounds = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

        cursorPosition.x = Mathf.Clamp(cursorPosition.x, minScreenBounds.x, maxScreenBounds.x);
        cursorPosition.y = Mathf.Clamp(cursorPosition.y, minScreenBounds.y, maxScreenBounds.y);
        cursorPosition.z = 0f;
    }

    public Vector3 GetCursorPosition()
    {
        return virtualCursorTransform != null ? virtualCursorTransform.position : Vector3.zero;
    }
}