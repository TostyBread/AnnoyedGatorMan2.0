using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class LevelDatas
{
    public Button levelButton;
    public Sprite levelImage;
    public bool isUnlocked;
}

public class LevelUnlockManager : MonoBehaviour
{
    public LevelDatas[] levelDatas;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < levelDatas.Length; i++)
        {
            if (levelDatas[i].isUnlocked)
            {
                if (levelDatas[i].levelImage != null) levelDatas[i].levelButton.image.sprite = levelDatas[i].levelImage;

                levelDatas[i].levelButton.interactable = true;
                levelDatas[i].levelButton.image.color = Color.white;

                foreach (Transform child in levelDatas[i].levelButton.transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
            else
            {
                levelDatas[i].levelButton.interactable = false;
            }
        }
    }
}