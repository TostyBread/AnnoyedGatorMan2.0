using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundVolumeManager : MonoBehaviour
{
    public Slider volumeSlider;
    public AudioSource backgroundMusicAudioSource;

    private void Start()
    {
        if (backgroundMusicAudioSource != null)
        {
            backgroundMusicAudioSource.volume = volumeSlider.value;
        }
    }

    public void ChangeBackgroundMusicVolume()
    {
        if (backgroundMusicAudioSource != null)
        {
            backgroundMusicAudioSource.volume = volumeSlider.value;
        }
    }
}
