using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CursorPhysicsHelper
{
    public static bool IsCursorOverTilemap(Vector2 position, float radius, LayerMask tilemapLayer)
    {
        return Physics2D.OverlapCircle(position, radius, tilemapLayer);
    }
}

public class ThrowDenied : MonoBehaviour
{
    private PlayerThrowManager playerThrowManager;
    private PlayerThrowManagerP2 playerThrowManagerP2;

    private bool mouseWasInside = false;
    private bool cursorWasInside = false;

    [SerializeField] private float detectionRadius = 0.05f;
    [SerializeField] private LayerMask tilemapLayer = default;

    void Start()
    {
        playerThrowManager = GetComponent<PlayerThrowManager>();
        playerThrowManagerP2 = GetComponent<PlayerThrowManagerP2>();
    }

    void Update()
    {
        Vector2 mousePos = ScreenToWorldPointMouse.Instance.GetMouseWorldPosition();
        Vector2 virtualCursorPos = PlayerAimController.Instance.GetCursorPosition();

        bool mouseNow = CursorPhysicsHelper.IsCursorOverTilemap(mousePos, detectionRadius, tilemapLayer);
        bool cursorNow = CursorPhysicsHelper.IsCursorOverTilemap(virtualCursorPos, detectionRadius, tilemapLayer);

        // Mouse tracking
        if (mouseNow && !mouseWasInside)
            Debug.Log("Mouse ENTERED tilemap collider.");
        else if (!mouseNow && mouseWasInside)
            Debug.Log("Mouse EXITED tilemap collider.");

        // Virtual cursor tracking
        if (cursorNow && !cursorWasInside)
            Debug.Log("Virtual Cursor ENTERED tilemap collider.");
        else if (!cursorNow && cursorWasInside)
            Debug.Log("Virtual Cursor EXITED tilemap collider.");

        mouseWasInside = mouseNow;
        cursorWasInside = cursorNow;
    }
}
