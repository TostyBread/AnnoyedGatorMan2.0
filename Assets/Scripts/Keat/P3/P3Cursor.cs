using UnityEngine;
using UnityEngine.InputSystem;


public class P3Cursor : MonoBehaviour
{
    P3Controls controls;
    public Vector2 P3AimMove;
    public Transform DefaultThrowPos;
    public float speed;
    public SpriteRenderer walkableArea; // Drag your Walkable sprite here in the inspector
    public float OffsetBetweenWall;
    private Vector2 clampOffsetMin = Vector2.zero;
    private Vector2 clampOffsetMax = Vector2.zero;

    private void Awake()
    {
        controls = new P3Controls();
        controls.Gameplay.AimMove.performed += context => P3AimMove = context.ReadValue<Vector2>();
        controls.Gameplay.AimMove.canceled += context => P3AimMove = Vector2.zero;

        clampOffsetMax = Vector2.one * OffsetBetweenWall;
        clampOffsetMin = Vector2.one * OffsetBetweenWall;
    }

    private void OnEnable()
    {
        if (Gamepad.current == null)
        {
            //gameObject.SetActive(false);
        }

        transform.parent = null;
        if (DefaultThrowPos != null)
        {
            transform.position = DefaultThrowPos.position;
        }

        controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        if (DefaultThrowPos != null && Gamepad.current != null)
        {
            transform.position = DefaultThrowPos.position;
        }
        controls.Gameplay.Disable();
    }

    private void Update()
    {
        Vector2 am = P3AimMove * speed * Time.deltaTime;
        transform.Translate(am);

        if (walkableArea != null)
        {
            Bounds bounds = walkableArea.bounds;

            Vector3 clampedPosition = transform.position;
            clampedPosition.x = Mathf.Clamp(
                clampedPosition.x,
                bounds.min.x + clampOffsetMin.x,
                bounds.max.x - clampOffsetMax.x
            );
            clampedPosition.y = Mathf.Clamp(
                clampedPosition.y,
                bounds.min.y + clampOffsetMin.y,
                bounds.max.y - clampOffsetMax.y
            );

            transform.position = clampedPosition;
        }
    }
}