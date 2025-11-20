using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameManager : MonoBehaviour
{
    public string[] AudioName;

    private void Awake()
    {
        if (AudioName.Length == 0) return;

        int randomIndex = Random.Range(0, AudioName.Length);
        AudioManager.Instance.PlaySound(AudioName[randomIndex], transform.position);
    }
}
