using UnityEngine;
using UnityEngine.UI;

public class BackgroundVolumeManager : MonoBehaviour
{
    public Slider volumeSlider;

    private AudioSource backgroundMusicAudioSource;
    public static float CurrentVolume { get; private set; } = 0.3f;
    private GameObject audioManager;

    private void Start()
    {
        if (AudioManager.Instance) audioManager = AudioManager.Instance.gameObject; // Get the AudioManager instance

        // Initialize slider with the authoritative static volume (do not overwrite it)
        if (volumeSlider != null)
        {
            volumeSlider.value = CurrentVolume;
            volumeSlider.onValueChanged.AddListener(ChangeBackgroundMusicVolume);
        }

        if (audioManager != null)
        {
            backgroundMusicAudioSource = audioManager.GetComponentInChildren<AudioSource>(); // Get the AudioSource from AudioManager
        }

        // Apply CurrentVolume to the audio source if it exists, but do not read from the audio source to set CurrentVolume.
        if (backgroundMusicAudioSource != null)
        {
            backgroundMusicAudioSource.volume = CurrentVolume;
        }
    }

    private void OnDestroy()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(ChangeBackgroundMusicVolume);
        }
    }

    // Called by the slider (float) and by other code (parameterless)
    public void ChangeBackgroundMusicVolume(float value)
    {
        CurrentVolume = Mathf.Clamp01(value);

        if (backgroundMusicAudioSource != null)
        {
            backgroundMusicAudioSource.volume = CurrentVolume;
        }
    }

    // Keep parameterless method for any UI events that call it without a parameter
    public void ChangeBackgroundMusicVolume()
    {
        if (volumeSlider != null)
        {
            ChangeBackgroundMusicVolume(volumeSlider.value);
        }
    }
}
