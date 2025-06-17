using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int scoreToClear = 0;

    private int currentScore = 0;
    public bool isCleared;

    private void Update()
    {
        if (currentScore >= scoreToClear && !isCleared)
        {
            isCleared = true;
        }
    }

    public void AddScore(int scoreToAdd)
    {
        currentScore += scoreToAdd;
    }
}
