using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelData : MonoBehaviour
{
    public int currentUnlockedLevel = 0;

    public LevelUnlockManager levelUnlockManager;

    public static LevelData Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Subscribe to sceneLoaded
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Always unsubscribe to avoid memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find LevelUnlockManager in the new scene
        levelUnlockManager = FindObjectOfType<LevelUnlockManager>();

        if (levelUnlockManager != null)
        {
            // Update the level unlock status based on currentUnlockedLevel
            for (int i = 0; i < levelUnlockManager.levelDatas.Length; i++)
            {
                levelUnlockManager.levelDatas[i].isUnlocked = i <= currentUnlockedLevel;
            }
        }
    }
}
