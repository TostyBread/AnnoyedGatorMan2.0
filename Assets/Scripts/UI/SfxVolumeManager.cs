using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SfxVolumeManager : MonoBehaviour
{
    public Slider volumeSlider;
    public AudioManager SfxAudioSource;

    private void Start()
    {
        if (SfxAudioSource != null)
        {
            SfxAudioSource.AdjustedVolume = volumeSlider.value;
        }
    }

    public void ChangeSfxVolume()
    {
        if (SfxAudioSource != null)
        {
            SfxAudioSource.AdjustedVolume = volumeSlider.value;
        }
    }
}
