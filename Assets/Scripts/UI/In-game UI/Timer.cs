using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SpecialMusicSettings
{
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0f, 5f)]
    public float fadeTime = 0.5f;
    public bool loop = false;
    public bool returnToPrevious = true;
}

public class Timer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float MaxTime = 10;
    public float RemainTime;
    public Image TimerBar;
    public bool UnlimitedTime = false;

    // Added audio queue for timer
    [Header("Audio Settings")]
    [SerializeField] private string minuteTickSound = "TimerMinute";
    [SerializeField] private string tenSecondTickSound = "TimerTenSecond";
    [SerializeField] private string secondTickSound = "TimerSecond";
    [SerializeField] private string gameOverSound = "GameOver";

    [Header("Special Music Settings")]
    [SerializeField] private SpecialMusicSettings finalSecondsMusic;
    [SerializeField] private SpecialMusicSettings gameOverMusic;

    private TMP_Text TimerText;
    private CameraMovement cameraMovement;
    private bool isWaiting = false;

    // Audio tracking variables
    private int lastMinute = -1;
    private int lastTenSecondInterval = -1;
    private int lastSecond = -1;
    private bool gameOverSoundPlayed = false;

    // Start is called before the first frame update
    void Start()
    {
        RemainTime = MaxTime;
        TimerText = GetComponentInChildren<TMP_Text>();
        cameraMovement = FindObjectOfType<CameraMovement>();

        // Initialize audio tracking
        lastMinute = Mathf.FloorToInt(RemainTime / 60f);
        lastTenSecondInterval = GetTenSecondInterval(RemainTime);
        lastSecond = Mathf.FloorToInt(RemainTime);
    }

    // Update is called once per frame
    void Update()
    {
        if (TimerBar != null) TimerBar.fillAmount = RemainTime / MaxTime;

        //Convert RemainTime to minutes and seconds
        int minutes = Mathf.FloorToInt(RemainTime / 60f);
        int seconds = Mathf.FloorToInt(RemainTime % 60f);
        if (TimerText != null) TimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        // Handle audio based on time intervals
        HandleAudioCues(minutes, seconds);

        // Only decrease time if not unlimited
        if (UnlimitedTime) return;

        if (cameraMovement != null && !cameraMovement.isMoving && !isWaiting && RemainTime > 0)
        {
            StartCoroutine(Wait(1f)); // Wait before reducing the time
        }
        else if (cameraMovement == null && RemainTime > 0 )
        {
            RemainTime -= Time.deltaTime;
        }

        RemainTime = Mathf.Clamp(RemainTime, 0, MaxTime);

        // Final seconds music trigger - using a threshold check instead of exact equality
        if (RemainTime <= 53f && RemainTime > 52.9f && finalSecondsMusic.clip != null)
        {
            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.PlaySpecialMusic(
                    finalSecondsMusic.clip,
                    perClipVolume: finalSecondsMusic.volume,
                    fadeTime: finalSecondsMusic.fadeTime,
                    loop: finalSecondsMusic.loop,
                    returnToPrevious: finalSecondsMusic.returnToPrevious
                );
            }
        }

        // Game over music trigger
        if (RemainTime <= 0 && !gameOverSoundPlayed && gameOverMusic.clip != null)
        {
            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.PlaySpecialMusic(
                    gameOverMusic.clip,
                    perClipVolume: gameOverMusic.volume,
                    fadeTime: gameOverMusic.fadeTime,
                    loop: gameOverMusic.loop,
                    returnToPrevious: gameOverMusic.returnToPrevious
                );
            }

            PlayGameOverSound();
            gameOverSoundPlayed = true;
        }

        if (Input.GetKeyDown(KeyCode.Delete)) 
        {
            RemainTime = 0; // For testing purposes
            gameOverSoundPlayed = false; // Reset for testing
        }
    }

    private void HandleAudioCues(int minutes, int seconds)
    {
        float totalSeconds = RemainTime;

        // Play sound every minute (when a full minute passes)
        if (totalSeconds > 60 && minutes != lastMinute && minutes >= 1)
        {
            PlayMinuteTickSound();
            lastMinute = minutes;
        }

        // When under 1 minute, play sound every 10 seconds
        else if (totalSeconds <= 60 && totalSeconds > 10)
        {
            int currentTenSecondInterval = GetTenSecondInterval(totalSeconds);
            if (currentTenSecondInterval != lastTenSecondInterval)
            {
                PlayTenSecondTickSound();
                lastTenSecondInterval = currentTenSecondInterval;
            }
        }

        // When under 10 seconds, play sound every second
        else if (totalSeconds <= 10 && totalSeconds > 0)
        {
            int currentSecond = Mathf.FloorToInt(totalSeconds);
            if (currentSecond != lastSecond && currentSecond >= 1)
            {
                PlaySecondTickSound();
                lastSecond = currentSecond;
            }
        }
    }

    private int GetTenSecondInterval(float time)
    {
        // Returns which 10-second interval we're in (50-60 = 5, 40-50 = 4, etc.)
        return Mathf.FloorToInt(time / 10f);
    }

    private void PlayMinuteTickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(minuteTickSound, transform.position);
        }
        else
        {
            Debug.LogWarning("AudioManager instance not found! Cannot play minute tick sound.");
        }
    }

    private void PlayTenSecondTickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(tenSecondTickSound, transform.position);
        }
        else
        {
            Debug.LogWarning("AudioManager instance not found! Cannot play ten-second tick sound.");
        }
    }

    private void PlaySecondTickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(secondTickSound, transform.position);
        }
        else
        {
            Debug.LogWarning("AudioManager instance not found! Cannot play second tick sound.");
        }
    }

    private void PlayGameOverSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(gameOverSound, transform.position);
        }
        else
        {
            Debug.LogWarning("AudioManager instance not found! Cannot play game over sound.");
        }
    }

    IEnumerator Wait(float waitTime)
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        RemainTime -= 1f;
        isWaiting = false;
    }
}
