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
    public static LevelUnlockManager Instance;

    public LevelData[] levelData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < levelData.Length; i++)
        {
            if (levelData[i].isUnlocked)
            {
                levelData[i].levelButton.interactable = true;
                levelData[i].levelButton.image.color = Color.white;

                foreach (Transform child in levelData[i].levelButton.transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
            else
            {
                levelData[i].levelButton.interactable = false;
            }
        }
    }
}
