using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class LevelSelectManager : MonoBehaviour
{
    public string levelSelected = "Tutorial";
    public int levelIndex = 0;

    [Header("References")]
    public TMP_Text levelText;
    public Image levelImage;
    public TMP_Text clearConditionText;

    public void ChangeLevelName(string name)
    {
        if (name != null) levelText.text = name;
    }

    public void ChangeLevelIndex(int index)
    {
        if (index >= 0) levelIndex = index;
    }

    public void ChangeLevelImage(Sprite image)
    {
        if (image != null) levelImage.sprite = image;
    }

    public void ChangeLevelCondition(string condition)
    {
        if (condition != null) clearConditionText.text = condition;
    }

    public void ReturnSelectedLevel(string selectedLevelName)
    {
        levelSelected = selectedLevelName;
    }
}
