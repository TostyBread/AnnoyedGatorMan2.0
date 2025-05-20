using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WeatherManager : MonoBehaviour
{
    public enum Weather { Normal, Rain, Sunny, Winter }

    public Weather weather;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (weather == Weather.Normal)
        {
            return;
        }
    }
}
