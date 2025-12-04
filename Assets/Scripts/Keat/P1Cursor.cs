using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P1Cursor : MonoBehaviour
{
    public SpriteRenderer defaultCursor;

    //Detact target reference
    private GameObject player;
    private DetectTarget detectTarget;

    private LevelLoader levelLoader;
    private ScoreManager scoreManager;
    private bool shouldShowSystemCursor;
    private Animator animator;

    private void Start()
    {
        levelLoader = FindObjectOfType<LevelLoader>();
        scoreManager = FindObjectOfType<ScoreManager>();

        if (defaultCursor != null)
            animator = defaultCursor.gameObject.GetComponent<Animator>();

        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            detectTarget = player.GetComponentInChildren<DetectTarget>();
    }

    void Update()
    {
        UpdateCursorVisibility();
        UpdateCursorPosition();

        CursorDetactItem();
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
    private bool CursorDetactItem()
    {
        if (detectTarget == null) return true;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] hits = Physics2D.OverlapPointAll(mousePos);

        foreach (var hit in hits)
        {
            foreach (var obj in detectTarget.AllItemInRange)
            {
                if (hit.gameObject == obj)
                {
                    animator.Play("OnItem_ControllerCursor");
                    return false;
                }
            }
        }

        // No hit, reset cursor
        animator.Play("Normal_ControllerCursor");
        return true;
    }
}
