using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public void PauseGame()
    {
        Time.timeScale = 0f; // Pause the game
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f; // Resume the game
    }
}
