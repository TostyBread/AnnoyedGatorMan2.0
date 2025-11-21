using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameManager : MonoBehaviour
{
    [Header("Audio setting")]
    public string[] AudioName;

    private void OnEnable ()
    {
        PlayRandomAudio();
    }

    private void PlayRandomAudio()
    {
        if (AudioName.Length == 0) return;

        int randomIndex = Random.Range(0, AudioName.Length);
        AudioManager.Instance.PlaySound(AudioName[randomIndex], transform.position);
    }
}
