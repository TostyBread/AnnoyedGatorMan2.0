using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockNextLevel : MonoBehaviour
{
    public int level;

    public void UnlockNextLevelFromButton()
    {
        LevelData levelData = FindObjectOfType<LevelData>();
        if (levelData != null)
        {
            levelData.UnlockNextLevel(level);
        }
    }
}
