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

    [Header("Rainy setting")]
    public float MinBlackOutInterval;
    public float MaxBlackOutInterval;
    public Light2D light2D;
    public string ThunderAudioName;
    private float currentBlackOutInterval;

    [Header("Hot setting")]
    public float heatMultiplier = 2;
    public GameObject SunRay;

    [Header("Cold setting")]
    public float coldMultiplier = 0.5f;
    public GameObject FreezeArea;

    [Header("References")]
    private DamageSource[] damageSources;
    private bool[] heatMultiplied;

    // Start is called before the first frame update
    void Start()
    {
        currentBlackOutInterval = UnityEngine.Random.Range(MinBlackOutInterval, MaxBlackOutInterval);
    }

    // Update is called once per frame
    void Update()
    {
        //Raniy
        if (weather == Weather.Rainy)
        {
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
                    damageSources[i].heatAmount *= heatMultiplier;
                    heatMultiplied[i] = true;
                }
            }
        }
        else if (weather != Weather.Hot)
        {
            SunRay.SetActive(false);
        }

        //Cold
        if (weather == Weather.Cold)
        {
            FreezeArea.SetActive(true);
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
                    damageSources[i].heatAmount *= coldMultiplier;
                    heatMultiplied[i] = true;
                }
            }
        }
        else if (weather != Weather.Cold)
        {
            SunRay.SetActive(false);
        }
    }
}
