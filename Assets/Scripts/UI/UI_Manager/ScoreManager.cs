using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public int scoreToClear = 0;

    public int currentScore = 0;
    public bool isCleared;

    private Timer timer;
    public GameObject WinScreen;
    public GameObject LoseScreen;

    private void Start()
    {
        timer = FindObjectOfType<Timer>();

        if (WinScreen != null) WinScreen.SetActive(false);
        if (LoseScreen != null) LoseScreen.SetActive(false);
    }

    private void Update()
    {
        if (currentScore >= scoreToClear && !isCleared)
        {
            isCleared = true;
        }

        CheckIfLevelCleared();
    }

    public void AddScore(int scoreToAdd)
    {
        currentScore += scoreToAdd;
    }

    private void CheckIfLevelCleared()
    {
        if (isCleared)
        {
            //Debug.Log("Level Cleared! Final Score: " + currentScore);
            StartCoroutine(DelayBeforeScreenShow(1f)); // Show win screen after a delay
        }
        else if (!isCleared && timer.RemainTime <= 0)
        {
            //Debug.Log("Level Failed! Final Score: " + currentScore);
            if (LoseScreen != null) LoseScreen.SetActive(true);
        }
    }

    IEnumerator DelayBeforeScreenShow(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (WinScreen != null) WinScreen.SetActive(true);
    }
}
