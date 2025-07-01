using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherIndicator : MonoBehaviour
{
    private static WeatherManager weatherManager;

    [Header("Weather Icon")]
    public GameObject NormalWeatherIcon;
    public GameObject RainyWeatherIcon;
    public GameObject HotWeatherIcon;
    public GameObject ColdWeatherIcon;

    // Start is called before the first frame update
    void Start()
    {
        weatherManager = FindObjectOfType<WeatherManager>();

        ResetWeatherIcon();
    }

    // Update is called once per frame
    void Update()
    {
        if (weatherManager.weather == WeatherManager.Weather.Normal)
        {
            ResetWeatherIcon();
            NormalWeatherIcon.SetActive(true);
        }
        else if (weatherManager.weather == WeatherManager.Weather.Rainy)
        {
            ResetWeatherIcon();
            RainyWeatherIcon.SetActive(true);
        }
        else if (weatherManager.weather == WeatherManager.Weather.Hot)
        {
            ResetWeatherIcon();
            HotWeatherIcon.SetActive(true);
        }
        else if (weatherManager.weather == WeatherManager.Weather.Cold)
        {
            ResetWeatherIcon();
            ColdWeatherIcon.SetActive(true);
        }
    }

    void ResetWeatherIcon()
    {
        NormalWeatherIcon.SetActive(false);
        RainyWeatherIcon.SetActive(false);
        HotWeatherIcon.SetActive(false);
        ColdWeatherIcon.SetActive(false);
    }
}
