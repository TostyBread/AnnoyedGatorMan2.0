using UnityEngine;
using UnityEngine.UI;

public class SfxVolumeManager : MonoBehaviour
{
    public Slider volumeSlider;

    private AudioManager SfxAudioSource;
    private static float currentVolume = 0.5f;
    private GameObject audioManager;

    private void Start()
    {
        if (AudioManager.Instance) audioManager = AudioManager.Instance.gameObject; // Get the AudioManager instance
        if (audioManager != null)
        {
            SfxAudioSource = audioManager.GetComponentInChildren<AudioManager>(); // Get the AudioManager component
        }

        volumeSlider.value = currentVolume; // Initialize slider with current volume

        if (SfxAudioSource != null)
        {
            SfxAudioSource.AdjustedVolume = volumeSlider.value;
        }
    }

    private void Update()
    {
        if (!audioManager || !SfxAudioSource) return;

        currentVolume = SfxAudioSource.AdjustedVolume; // Update current volume
    }

    public void ChangeSfxVolume()
    {
        if (SfxAudioSource != null)
        {
            SfxAudioSource.AdjustedVolume = volumeSlider.value;
        }
    }
}
