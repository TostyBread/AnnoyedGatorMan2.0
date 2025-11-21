using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("Score setting")]
    public int scoreToClear = 0;
    public int currentScore = 0;

    //Saparate variable to track who last added the score
    public GameObject lastHolder;
    private int player1Score = 0;
    private int player2Score = 0;

    [Header("Level clear setting")]
    public int currentLevelIndex;
    public bool isCleared;
    public bool willUnlockNextLevel = false;

    private Timer timer;
    private LevelData levelData;

    [Header("Endgame Screen References")]
    public GameObject WinScreen;
    public GameObject LoseScreen;
    public GameObject[] ConfettiEffect;

    public bool gameOver; // for P1 cursor management

    private void Start()
    {
        timer = FindObjectOfType<Timer>();
        levelData = FindObjectOfType<LevelData>();

        if (WinScreen != null) WinScreen.SetActive(false);
        if (LoseScreen != null) LoseScreen.SetActive(false);

        if (levelData != null) levelData.willUnlockNextLevel = willUnlockNextLevel;

        if (ConfettiEffect != null)
        {
            foreach (GameObject confetti in ConfettiEffect)
            {
                confetti.SetActive(false);
            }
        }
        

        gameOver = false;
    }

    private void Update()
    {
        if (currentScore >= scoreToClear && !isCleared)
        {
            isCleared = true;
        }

        CheckIfLevelCleared();

        if (Input.GetKeyDown(KeyCode.Insert)) currentScore = scoreToClear; // For testing purposes
    }

    public void AddScore(int scoreToAdd, GameObject lastHolder)
    {
        currentScore += scoreToAdd;
        this.lastHolder = lastHolder;

        if (lastHolder.name == "Player1")
        {
            player1Score += scoreToAdd;
        }
        else if (lastHolder.name == "Player3")
        {
            player2Score += scoreToAdd;
        }

        Debug.Log("Current Player1 Score: " + player1Score);
        Debug.Log("Current Player2 Score: " + player2Score);
    }

    private void CheckIfLevelCleared()
    {
        if (isCleared && timer.RemainTime <= 0)
        {
            //Debug.Log("Level Cleared! Final Score: " + currentScore);
            StartCoroutine(DelayBeforeScreenShow(1f)); // Show win screen after a delay

            gameOver = true; 
        }
        else if (!isCleared && timer.RemainTime <= 0)
        {
            //Debug.Log("Level Failed! Final Score: " + currentScore);
            if (LoseScreen != null)
            {
                LoseScreen.SetActive(true);

                UpdateEndScreenScoreText();
                EnableConfettiEffects();
            }

            gameOver = true;
        }
    }

    IEnumerator DelayBeforeScreenShow(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (WinScreen != null)
        {
            WinScreen.SetActive(true);

            UpdateEndScreenScoreText();
            EnableConfettiEffects();
        }

        if (levelData != null)
        {
            levelData.UnlockNextLevel(currentLevelIndex);
            levelData.SaveHighScore(currentLevelIndex, currentScore);
        }
    }

    private void UpdateEndScreenScoreText()
    {
        TMP_Text finalScoreText = WinScreen.GetComponentInChildren<TMPro.TMP_Text>();
        if (finalScoreText != null)
        {
            finalScoreText.text =
                "Total Score: " + currentScore.ToString() + "\n" +
                "Player1: " + player1Score.ToString() + "\n" +
                "Player2: " + player2Score.ToString();
        }
    }

    private void EnableConfettiEffects()
    {
        foreach (GameObject confetti in ConfettiEffect)
        {
            confetti.SetActive(true);
        }
    }
}
