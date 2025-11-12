using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockNextLevel : MonoBehaviour
{
    public int level;
    private ScoreManager scoreManager;

    private void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
    }

    public void UnlockNextLevelFromButton()
    {
        LevelData levelData = FindObjectOfType<LevelData>();
        if (levelData != null)
        {
            levelData.UnlockNextLevel(level);
        }
        scoreManager.gameOver = true;
    }
}
