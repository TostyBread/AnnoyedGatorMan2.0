using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class P3Cursor : MonoBehaviour
{
    public static P3Cursor Instance { get; private set; }

    P3Controls controls;
    public Vector2 P3AimMove;
    public Transform DefaultThrowPos;
    public float speed;
    public SpriteRenderer walkableArea;
    public float OffsetBetweenWall;
    private Vector2 clampOffsetMin = Vector2.zero;
    private Vector2 clampOffsetMax = Vector2.zero;

    public GameObject targetPlayer;

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        controls = new P3Controls();
        controls.Gameplay.AimMove.performed += context => P3AimMove = context.ReadValue<Vector2>();
        controls.Gameplay.AimMove.canceled += context => P3AimMove = Vector2.zero;

        clampOffsetMax = Vector2.one * OffsetBetweenWall;
        clampOffsetMin = Vector2.one * OffsetBetweenWall;
    }
    private void Start()
    {
        targetPlayer = GameObject.Find("Player3");
        if (targetPlayer != null)
        {
            foreach (Transform item in targetPlayer.transform.GetComponentsInChildren<Transform>())
            {
                if (item.name.Contains("P3_"))
                {
                    DefaultThrowPos = item;
                }
            }
        }

        StartCoroutine(WaitBeforeDisable(0.2f)); //try to avoid null if other GameObject try to get this gameObject at Start
    }

    private void OnEnable()
    {
        if (controls == null) return;

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
        if (controls == null) return;

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

    // Add helper method to get cursor position
    public Vector2 GetCursorPosition()
    {
        return transform.position;
    }

    private IEnumerator WaitBeforeDisable(float second)
    {
        yield return new WaitForSeconds(second);
        gameObject.SetActive(false);
    }
}