using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WeatherManager : MonoBehaviour
{
    public enum Weather { Normal, Rainy, Hot, Cold }

    public Weather weather;
    private int randomWeather;

    [Header("Rainy setting")]
    public float MinBlackOutInterval;
    public float MaxBlackOutInterval;
    public Light2D light2D;
    public string ThunderAudioName;
    public AudioClip RainingClip;
    private float currentBlackOutInterval;

    [Header("Hot setting")]
    public float heatMultiplier = 2;
    public GameObject SunRay;
    public AudioClip SunnyClip;

    [Header("Cold setting")]
    public float coldMultiplier = 0.5f;
    public GameObject FreezeArea;
    public AudioClip WinterClip;

    [Header("References")]
    private AudioSource audioSource;
    private DamageSource[] damageSources;
    private bool[] heatMultiplied;

    // Start is called before the first frame update
    void Start()
    {
        currentBlackOutInterval = UnityEngine.Random.Range(MinBlackOutInterval, MaxBlackOutInterval);
        audioSource = GetComponent<AudioSource>();

        randomWeather = UnityEngine.Random.Range(1,5);

        switch (randomWeather) 
        {
            case 1:
                weather = Weather.Normal;
                break;

            case 2:
                weather = Weather.Rainy; 
                break;

            case 3:
                weather = Weather.Hot;
                break;

            case 4:
                weather = Weather.Cold;
                break;
        }

        Debug.Log("Weather: " + weather);
    }

    // Update is called once per frame
    void Update()
    {
        if (weather == Weather.Normal)
        {          
            if (audioSource.isPlaying) audioSource.Stop();
            audioSource.clip = null;

            ResetFireHeat();
        }

        //Raniy
        if (weather == Weather.Rainy)
        {
            if (audioSource.clip != RainingClip)
            {
                audioSource.clip = RainingClip;
                audioSource.Play();                
            }

            ResetFireHeat();

            if (currentBlackOutInterval > 0)
            {
                currentBlackOutInterval -= Time.deltaTime;
            }

            if (currentBlackOutInterval <= 0)
            {
                light2D.intensity = 0.02f;
                AudioManager.Instance.PlaySound(ThunderAudioName, 1.0f, transform.position);
                currentBlackOutInterval = UnityEngine.Random.Range(MinBlackOutInterval, MaxBlackOutInterval);
            }
        }

        //Hot
        if (weather == Weather.Hot)
        {
            SunRay.SetActive(true);

            if (audioSource.clip != SunnyClip)
            {
                audioSource.clip = SunnyClip;
                audioSource.Play();
            }

            AdjustFireHeat(heatMultiplier);
        }
        else if (weather != Weather.Hot)
        {
            SunRay.SetActive(false);
        }

        //Cold
        if (weather == Weather.Cold)
        {
            FreezeArea.SetActive(true);

            if (audioSource.clip != WinterClip)
            {
                audioSource.clip = WinterClip;
                audioSource.Play();
            }

            AdjustFireHeat(coldMultiplier);
        }
        else if (weather != Weather.Cold)
        {
            FreezeArea.SetActive(false);
        }
    }

    private void AdjustFireHeat(float multiplier)
    {
        GameObject[] fires = GameObject.FindGameObjectsWithTag("Fire");
        damageSources = new DamageSource[fires.Length];

        if (heatMultiplied == null || heatMultiplied.Length != fires.Length)
        {
            heatMultiplied = new bool[fires.Length];
        }

        for (int i = 0; i < fires.Length; i++)
        {
            damageSources[i] = fires[i].GetComponent<DamageSource>();

            if (!heatMultiplied[i])
            {
                damageSources[i].heatAmount *= multiplier;
                heatMultiplied[i] = true;
            }
        }
    }

    private void ResetFireHeat()
    {
        GameObject[] fires = GameObject.FindGameObjectsWithTag("Fire");
        foreach (var fire in fires)
        {
            DamageSource damageSource = fire.GetComponent<DamageSource>();
            if (damageSource != null) damageSource.ResetHeat();
        }
    }
}
