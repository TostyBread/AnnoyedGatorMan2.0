using System.Collections.Generic;
using UnityEngine;

public class AudioClipLoader : MonoBehaviour
{
    public List<AudioClip> clips; // Drag & drop sounds in Inspector

    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.RegisterSounds(clips);
        }
    }
}