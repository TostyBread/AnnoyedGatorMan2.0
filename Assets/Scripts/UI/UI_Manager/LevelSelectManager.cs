using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelSelectManager : MonoBehaviour
{
    public int levelSelected = 0;

    [Header("References")]
    public TMP_Text levelText;
    public Image levelImage;
    public TMP_Text clearConditionText;

    public void ChangeLevelName(string name)
    {
        if (name != null) levelText.text = name;
    }

    public void ChangeLevelImage(Sprite image)
    {
        if (image != null) levelImage.sprite = image;
    }

    public void ChangeLevelCondition(string condition)
    {
        if (condition != null) clearConditionText.text = condition;
    }

    public void ReturnSelectedLevel(int levelIndex)
    {
        levelSelected = levelIndex;
    }
}
