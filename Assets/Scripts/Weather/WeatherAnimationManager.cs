using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WeatherAnimationManager : MonoBehaviour
{
    private WeatherManager weatherManager;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        weatherManager = FindAnyObjectByType<WeatherManager>();
        animator = GetComponent<Animator>();

        ResetWeatherAnimation();
    }

    // Update is called once per frame
    void Update()
    {
        if (weatherManager.weather == WeatherManager.Weather.Normal)
        {
            ResetWeatherAnimation();
            animator.SetBool("Normal", true);
        }
        else if (weatherManager.weather == WeatherManager.Weather.Hot)
        {
            ResetWeatherAnimation();
            animator.SetBool("Hot", true);
        }
        else if (weatherManager.weather == WeatherManager.Weather.Cold)
        {
            ResetWeatherAnimation();
            animator.SetBool("Cold", true);
        }
        else if (weatherManager.weather == WeatherManager.Weather.Rainy)
        {
            ResetWeatherAnimation();
            animator.SetBool("Rain", true);
        }
    }

    private void ResetWeatherAnimation()
    {
        animator.SetBool("Normal", false);
        animator.SetBool("Hot", false);
        animator.SetBool("Cold", false);
        animator.SetBool("Rain", false);
    }
}
