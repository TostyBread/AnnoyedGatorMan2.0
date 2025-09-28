using UnityEngine;
public class PlayerAimController : MonoBehaviour
{
    public static PlayerAimController Instance { get; private set; }

    public RectTransform uiCursorRect;
    public Canvas canvas;
    public float cursorSpeed = 1000f;
    public float aimDeadzone = 0.2f;
    public LayerMask softLockLayers;
    public float lockOnRadius = 0.5f;

    private Vector2 cursorScreenPosition;
    private Camera mainCamera;
    private PlayerInputActions inputActions;
    private Transform lockedTarget;

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
}