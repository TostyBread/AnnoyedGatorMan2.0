using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P1Cursor : MonoBehaviour
{
    public SpriteRenderer cursor;
    private LevelLoader levelLoader;
    private bool shouldShowSystemCursor;

    private void Start()
    {
        levelLoader = FindObjectOfType<LevelLoader>();
    }

    void Update()
    {
        shouldShowSystemCursor = levelLoader.isShowingSettingScreen;

        Cursor.visible = shouldShowSystemCursor;
        cursor.enabled = !shouldShowSystemCursor;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f; // adjust based on camera distance
        cursor.transform.position = Camera.main.ScreenToWorldPoint(mousePos);

    }
}
