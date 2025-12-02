using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Unity.VisualScripting;

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
    public Image enemyDumpsterIcon;
    public Image backgroundDumpsterIcon;
    public Sprite flySprite;
    public Sprite mosquitoeSprite;
    public Sprite cockroachSprite;
    public Sprite mouseSprite;

    [Header("Timer")]
    public float DelayBeforeShowing = 0f;
    public float ShowingTime = 3f;

    [Header("Pregame Setting")]
    public GameObject PregameScreen;
    public bool showPregameUI = true;
    public bool isShowing = false; // for level loader setting screen 

    [Header("References")]
    private WeatherManager weatherManager;

    void Awake()
    {
        weatherManager = FindObjectOfType<WeatherManager>();

        EnemySpawner spawner = FindAnyObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            Canvas canvas = spawner.GetComponentInChildren<Canvas>(true);
            if (canvas != null)
            {
                Image[] images = canvas.GetComponentsInChildren<Image>(true);

                foreach (Image img in images)
                {
                    if (img.name == "Enemy Dumpster Icon")
                    {
                        enemyDumpsterIcon = img;
                        break;
                    }
                }

                foreach (Image img in images)
                {
                    if (img.name == "Background Dumpster Icon")
                    {
                        backgroundDumpsterIcon = img;
                        break;
                    }
                }
            }
        }


        PregameScreen.SetActive(false);

        if (weatherManager != null && showPregameUI == true)
        {
             StartCoroutine(ShowPregameUI());
        }
    }

    IEnumerator ShowPregameUI()
    {
        yield return new WaitForSeconds(DelayBeforeShowing);

        if (weatherManager == null) yield break;

        isShowing = true;

        PregameScreen.SetActive(true);

        switch (weatherManager.weather)
        {
            case WeatherManager.Weather.Normal:
                weatherIcon.sprite = normalSprite;
                enemyIcon.sprite = flySprite;
                enemyDumpsterIcon.sprite = flySprite;
                backgroundDumpsterIcon.sprite = flySprite;
                break;
            case WeatherManager.Weather.Hot:
                weatherIcon.sprite = sunnySprite;
                enemyIcon.sprite = mosquitoeSprite;
                enemyDumpsterIcon.sprite = mosquitoeSprite;
                backgroundDumpsterIcon.sprite = mosquitoeSprite;
                break;
            case WeatherManager.Weather.Rainy:
                weatherIcon.sprite = rainySprite;
                enemyIcon.sprite = cockroachSprite;
                enemyDumpsterIcon.sprite= cockroachSprite;
                backgroundDumpsterIcon.sprite = cockroachSprite;
                break;
            case WeatherManager.Weather.Cold:
                weatherIcon.sprite = snowySprite;
                enemyIcon.sprite = mouseSprite;
                enemyDumpsterIcon.sprite = mouseSprite;
                backgroundDumpsterIcon.sprite = mouseSprite;
                break;
        }

        yield return new WaitForSeconds(ShowingTime);

        isShowing = false;
        PregameScreen.SetActive(false);
    }
}
