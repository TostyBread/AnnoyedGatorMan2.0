using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using Random = UnityEngine.Random;


public class EnemySpawner : MonoBehaviour
{
    public List<GameObject> Enemies = new List<GameObject>();
    public GameObject UI;

    public float MaxSpawnedEnemy = 5;
    public int MinSpawnTime = 25;
    public int MaxSpawnTime = 30;

    private Timer timer;
    private Sanity sanity;
    public List<Transform> Spawners = new List<Transform>();

    [Header("do not touch, just for Refrence")]
    [SerializeField] private float ChargeReadyTime;
    [SerializeField] private float ChargeTimer;
    public float currentSpawnedEnemy = 0;

    bool SanityIsEmptyOnce;

    private Transform spawner;
    public WeatherManager weatherManager;
    [Header("do not touch, just for Refrence")]
    public GameObject EnemyForCurrentWeather;

    // Start is called before the first frame update
    void Start()
    {
        ChargeTimer = 0;

        if (UI == null)
        {
            UI = GameObject.Find("UI");
            Debug.LogWarning("UI is missing, So " + gameObject + " will find gameObject name: UI");
        }

        timer = UI.GetComponentInChildren<Timer>();
        sanity = UI.GetComponentInChildren<Sanity>();
        ChargeReadyTime = Random.Range(MinSpawnTime, MaxSpawnTime);
        weatherManager = FindAnyObjectByType<WeatherManager>();

        if (Spawners.Count == 0)
        {
            Transform[] getSpawners = GetComponentsInChildren<Transform>();
            if (getSpawners.Length > 0)
            {
                foreach (Transform spawner in getSpawners)
                {
                    if (spawner.gameObject == gameObject) //Prevent spawners from getting this gameObject
                        continue;

                    Spawners.Add(spawner);
                }
            }
        }

        EnemyForCurrentWeather = GetEnemyByWeather();
    }

    // Update is called once per frame
    void Update()
    {
        if (SanityIsEmptyOnce == false && sanity.RemainSanity == 0)
        {
            foreach (Transform spawner in Spawners)
            {
                GameObject enemy = Instantiate(EnemyForCurrentWeather, spawner.position, spawner.rotation);
            }
            ChargeReadyTime = Random.Range(5, 10);
            ChargeTimer = 0;
            SanityIsEmptyOnce = true;
        }

        SpawnEnemy();
    }

    void SpawnEnemy()
    {
        if (currentSpawnedEnemy >= MaxSpawnedEnemy) return;

        ChargeTimer += Time.deltaTime;

        if (ChargeTimer >= ChargeReadyTime)
        {
            Debug.Log("Charge Completed");

            Debug.Log("Let's spawn enemy");
            currentSpawnedEnemy++;

            try
            {
                spawner = Spawners[Random.Range(0, Spawners.Count)]; // Randomly select a spawner from the list
            }
            catch (ArgumentOutOfRangeException)
            {
                Debug.LogWarning("There is no index in Spawners arrary");
            }

            GameObject enemy = Instantiate(EnemyForCurrentWeather, spawner.position, spawner.rotation);
            ChargeReadyTime = Random.Range(MinSpawnTime, MaxSpawnTime);

            if (sanity.RemainSanity <= 0) //If sanity is empty...
            {
                if (SanityIsEmptyOnce == false) // All spawner will spawn enemy once
                {
                    foreach (Transform spawnPos in Spawners)
                    {
                        Instantiate(EnemyForCurrentWeather, spawner.position, spawner.rotation);
                    }
                    SanityIsEmptyOnce = true;
                }

                ChargeReadyTime /= 2;  // enemy spawn speed is doubled
            }

            ChargeTimer = 0;
        }
    }

    GameObject GetEnemyByWeather()
    {
        if (weatherManager.weather == WeatherManager.Weather.Normal)
            return Enemies.Find(e => e.name.Contains("Fly"));
        else if (weatherManager.weather == WeatherManager.Weather.Rainy)
            return Enemies.Find(e => e.name.Contains("Cockroach"));
        else if (weatherManager.weather == WeatherManager.Weather.Cold)
            return Enemies.Find(e => e.name.Contains("Mouse"));
        else if (weatherManager.weather == WeatherManager.Weather.Hot)
            return Enemies.Find(e => e.name.Contains("Mosquitoe"));

        return null;
    }

}


