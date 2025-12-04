using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P1Cursor : MonoBehaviour
{
    public SpriteRenderer defaultCursor;

    [Header("Hover Settings")]
    public string[] hoverTags; // Tags to detect on hover

    private LevelLoader levelLoader;
    private ScoreManager scoreManager;
    private Animator animator;
    private bool shouldShowSystemCursor;

    private void Start()
    {
        levelLoader = FindObjectOfType<LevelLoader>();
        scoreManager = FindObjectOfType<ScoreManager>();

        if (defaultCursor != null)
            animator = defaultCursor.gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        UpdateCursorVisibility();
        UpdateCursorPosition();
        DetectHover();
    }

    private void UpdateCursorVisibility()
    {
        if (scoreManager != null && scoreManager.gameOver)
        {
            shouldShowSystemCursor = true;
        }
        else
        {
            shouldShowSystemCursor = levelLoader.isShowingSettingScreen;
        }

        Cursor.visible = shouldShowSystemCursor;
        defaultCursor.enabled = !shouldShowSystemCursor;
    }

    private void UpdateCursorPosition()
    {
        if (defaultCursor == null) return;

        // Get world position from existing singleton helper
        Vector3 worldPos = ScreenToWorldPointMouse.Instance.GetMouseWorldPosition();
        defaultCursor.transform.position = worldPos;
    }

    private void DetectHover()
    {
        if (hoverTags == null || hoverTags.Length == 0) return;

        // For 2D objects
        Vector3 mouseWorldPos = ScreenToWorldPointMouse.Instance.GetMouseWorldPosition();
        Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

        if (hit.collider != null)
        {
            foreach (var tag in hoverTags)
            {
                if (hit.collider.CompareTag(tag))
                {
                    Debug.Log("Mouse hovering on: " + hit.collider.name + " with tag: " + tag);
                    break; // Stop after first match
                }
            }
        }
    }
}
