using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class PregameUI : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Weather Icon")]
    public Image weatherIcon;
    public Sprite normalSprite;
    public Sprite sunnySprite;
    public Sprite rainySprite;
    public Sprite snowySprite;

    [Header("Enemy Icon")]
    public Image enemyIcon;
    public Sprite flySprite;
    public Sprite mosquitoeSprite;
    public Sprite cockroachSprite;
    public Sprite mouseSprite;

    [Header("Timer")]
    public float DelayBeforeShowing = 0f;
    public float ShowingTime = 3f;

    [Header("References")]
    public GameObject PregameScreen;
    private WeatherManager weatherManager;

    void Awake()
    {
        weatherManager = FindObjectOfType<WeatherManager>();
        PregameScreen.SetActive(false);

        if (weatherManager != null)
        {
             StartCoroutine(ShowPregameUI());
        }
    }

    IEnumerator ShowPregameUI()
    {
        yield return new WaitForSeconds(DelayBeforeShowing);

        if (weatherManager == null) yield break;

        PregameScreen.SetActive(true);

        switch (weatherManager.weather)
        {
            case WeatherManager.Weather.Normal:
                weatherIcon.sprite = normalSprite;
                enemyIcon.sprite = flySprite;
                break;
            case WeatherManager.Weather.Hot:
                weatherIcon.sprite = sunnySprite;
                enemyIcon.sprite = mosquitoeSprite;
                break;
            case WeatherManager.Weather.Rainy:
                weatherIcon.sprite = rainySprite;
                enemyIcon.sprite = cockroachSprite;
                break;
            case WeatherManager.Weather.Cold:
                weatherIcon.sprite = snowySprite;
                enemyIcon.sprite = mouseSprite;
                break;
        }

        yield return new WaitForSeconds(ShowingTime);
        PregameScreen.SetActive(false);
    }
}
