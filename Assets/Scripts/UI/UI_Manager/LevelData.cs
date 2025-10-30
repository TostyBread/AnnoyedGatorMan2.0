using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelData : MonoBehaviour
{
    public int currentUnlockedLevel = 0;
    public bool willUnlockNextLevel = false;

    [Header("References")]
    public LevelSelectManager levelSelectManager;
    public LevelUnlockManager levelUnlockManager;
    public TMP_Text highestScore;

    public static LevelData Instance;

    private const string SaveKey = "UnlockedLevel";
    private const string HighScoreKey = "HighScore_Level_";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Load saved data
            currentUnlockedLevel = PlayerPrefs.GetInt(SaveKey, currentUnlockedLevel);

            // Subscribe to sceneLoaded
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            ResetAllData();
            Debug.Log("All progress reset!");
        }

        if (highestScore != null && levelSelectManager != null)
        {
            highestScore.text = "Highest score: " + GetHighScore(levelSelectManager.levelIndex).ToString();
        }
    }

    public void ResetAllData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        currentUnlockedLevel = 0;
    }

    private void OnDestroy()
    {
        // Always unsubscribe to avoid memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnApplicationQuit()
    {
        SaveProgress();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) SaveProgress();
    }

    public void UnlockNextLevel(int levelIndex)
    {
        if (levelIndex >= currentUnlockedLevel && willUnlockNextLevel)
        {
            currentUnlockedLevel = levelIndex + 1;
            PlayerPrefs.SetInt(SaveKey, currentUnlockedLevel);
            PlayerPrefs.Save();
        }
    }

    private void SaveProgress()
    {
        PlayerPrefs.SetInt(SaveKey, currentUnlockedLevel);
        PlayerPrefs.Save();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find LevelUnlockManager in the new scene
        levelSelectManager = FindObjectOfType<LevelSelectManager>();
        levelUnlockManager = FindObjectOfType<LevelUnlockManager>();

        GameObject highScoreObj = GameObject.FindGameObjectWithTag("HighScoreText");
        if (highScoreObj != null)
        {
            highestScore = highScoreObj.GetComponent<TMP_Text>();
        }
        else
        {
            highestScore = null;
        }

        if (levelUnlockManager != null)
        {
            // Update the level unlock status based on currentUnlockedLevel
            for (int i = 0; i < levelUnlockManager.levelDatas.Length; i++)
            {
                levelUnlockManager.levelDatas[i].isUnlocked = i <= currentUnlockedLevel;
            }
        }
    }

    public void SaveHighScore(int levelIndex, int score)
    {
        int savedScore = PlayerPrefs.GetInt(HighScoreKey + levelIndex, 0);
        if (score > savedScore)
        {
            PlayerPrefs.SetInt(HighScoreKey + levelIndex, score);
            PlayerPrefs.Save();
        }
    }

    public int GetHighScore(int levelIndex)
    {
        return PlayerPrefs.GetInt(HighScoreKey + levelIndex, 0);
    }
}
