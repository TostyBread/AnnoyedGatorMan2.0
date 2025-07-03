using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundVolumeManager : MonoBehaviour
{
    public Slider volumeSlider;
    public AudioSource backgroundMusicAudioSource;
    private static float currentVolume = 0.5f;
    public GameObject audioManager;

    private void Start()
    {
        audioManager = AudioManager.Instance.gameObject; // Get the AudioManager instance
        if (audioManager != null)
        {
            backgroundMusicAudioSource = audioManager.GetComponentInChildren<AudioSource>(); // Get the AudioSource from AudioManager
        }

        if (backgroundMusicAudioSource != null)
        {
            backgroundMusicAudioSource.volume = volumeSlider.value;
        }

        volumeSlider.value = currentVolume; // Initialize slider with current volume
    }

    private void Update()
    {
        currentVolume = backgroundMusicAudioSource.volume; // Update current volume
    }

    public void ChangeBackgroundMusicVolume()
    {
        if (backgroundMusicAudioSource != null)
        {
            backgroundMusicAudioSource.volume = volumeSlider.value;
        }
    }
}
