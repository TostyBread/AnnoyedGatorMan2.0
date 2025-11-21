using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameManager : MonoBehaviour
{
    [Header("Scale setting")]
    [SerializeField] private Vector3 MinScale;
    [SerializeField] private float ScaleDuration = 1f;
    private Vector3 initialScale;

    [Header("Audio setting")]
    public string[] AudioName;

    private void Awake()
    {
        transform.localScale = MinScale;
    }

    private void OnEnable ()
    {
        StartScaling();
        PlayRandomAudio();
    }

    public void StartScaling()
    {
        initialScale = transform.localScale;
        StopAllCoroutines();
        StartCoroutine(ScaleUpCoroutine());
    }
    private IEnumerator ScaleUpCoroutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime < ScaleDuration)
        {
            transform.localScale = Vector3.Lerp(initialScale, Vector3.one, elapsedTime / ScaleDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localScale = Vector3.one; // Ensure it ends exactly at full scale
    }

    private void PlayRandomAudio()
    {
        if (AudioName.Length == 0) return;

        int randomIndex = Random.Range(0, AudioName.Length);
        AudioManager.Instance.PlaySound(AudioName[randomIndex], transform.position);
    }
}
