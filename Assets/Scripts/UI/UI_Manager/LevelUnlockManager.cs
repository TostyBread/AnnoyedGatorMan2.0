using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class LevelData
{
    public Button levelButton;
    public bool isUnlocked;
}

public class LevelUnlockManager : MonoBehaviour
{
    public LevelData[] levelData;
    private int ClearedLevel = 0;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < levelData.Length; i++)
        {
            if (levelData[i].isUnlocked)
            {
                levelData[i].levelButton.interactable = true;
            }
            else
            {
                levelData[i].levelButton.interactable = false;
            }
        }
    }

    public void UnlockNextLevel()
    {
        if (ClearedLevel < levelData.Length - 1)
        {
            ClearedLevel++;
            levelData[ClearedLevel].isUnlocked = true;
            levelData[ClearedLevel].levelButton.interactable = true;
        }
    }
}
