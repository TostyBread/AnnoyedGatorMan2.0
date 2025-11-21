using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P1Cursor : MonoBehaviour
{
    public SpriteRenderer defaultCursor;
    public SpriteRenderer interactCursor;

    private LevelLoader levelLoader;
    private ScoreManager scoreManager;
    private bool shouldShowSystemCursor;

    private void Start()
    {
        levelLoader = FindObjectOfType<LevelLoader>();
        scoreManager = FindObjectOfType<ScoreManager>();
    }

    void FixedUpdate()
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

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f; // adjust based on camera distance
        defaultCursor.transform.position = ScreenToWorldPointMouse.Instance.GetMouseWorldPosition(); // Reference existing script to reduce redundancy.

    }
}
