using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class EnemySpawner : MonoBehaviour
{
    public List<GameObject> Enemies = new List<GameObject>();
    public GameObject UI;

    [SerializeField] private Dumpster dumpster;
    private Transform dumpsterPos;
    [SerializeField] private GameObject canvas;
    [SerializeField] private Image EnemyRadiusbar;
    [SerializeField] private Vector2 offset;
    [SerializeField] private GameObject EnemySpawnEffect;
    [SerializeField] private Transform EnemySpawnEffectPos;

    public float MaxSpawnedEnemy = 5;
    public int MinSpawnTime = 25;
    public int MaxSpawnTime = 30;

    private Timer timer;
    private Sanity sanity;
    public List<Transform> Spawners = new List<Transform>();

    public int CountDeadBody = 0;
    private float spawnSpeedTimer;

    public int CountBurntFood = 0;
    public int CountTrashbag = 0;
    public int CountTotalObstacleObject = 0;
    
    [Header("do not touch, just for Refrence")]
    [SerializeField] private float ChargeReadyTime;
    [SerializeField] private float ChargeTimer;
    public int currentSpawnedEnemy = 0;

    bool SanityIsEmptyOnce;

    public Transform spawner;
    [Header("do not touch, just for Refrence")]
    public WeatherManager weatherManager;
    public GameObject EnemyForCurrentWeather;

    [SerializeField] private bool stopAndHideUiBar;

 
    // Start is called before the first frame update
    void Start()
    {
        ChargeTimer = 0;

        if (UI == null)
        {
            UI = GameObject.Find("UI");
            //Debug.LogWarning("UI is missing, So " + gameObject + " will find gameObject name: UI");
        }

        timer = UI.GetComponentInChildren<Timer>();
        sanity = UI.GetComponentInChildren<Sanity>();
        ChargeReadyTime = Random.Range(5, 7);
        weatherManager = FindAnyObjectByType<WeatherManager>();

        dumpster = FindAnyObjectByType<Dumpster>();
        dumpsterPos = dumpster.gameObject.transform;

        canvas.transform.position = new Vector2(dumpsterPos.position.x + offset.x, dumpsterPos.position.y + offset.y);

        if (Spawners.Count == 0)
        {
            Transform[] getSpawners = GetComponentsInChildren<Transform>();
            if (getSpawners.Length > 0)
            {
                foreach (Transform spawner in getSpawners)
                {
                    if (spawner.gameObject == gameObject || !spawner.name.Contains("Spawner")) //Prevent spawners from getting this gameObject && other non-spawner
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
        if (EnemyRadiusbar != null)
        {
            EnemyRadiusbar.fillAmount = ChargeTimer / ChargeReadyTime;
            if (EnemyRadiusbar.fillAmount == 1)
            {
                EnemyRadiusbar.fillAmount = 0;
            }
        }

        if (stopAndHideUiBar)
        {
            canvas.SetActive(false);
        }
        else
        {
            canvas.SetActive(true);
        }

        if (SanityIsEmptyOnce == false && sanity.RemainSanity == 0) //all spawner spawn enemy at once once player die
        {
            foreach (Transform spawner in Spawners)
            {
                SpawnEnemy(spawner);
            }

            ChargeReadyTime = Random.Range(MinSpawnTime / 2, MaxSpawnTime / 2);
            ChargeTimer = 0; SanityIsEmptyOnce = true;
        }

        CountObstacleObjectInGame();
        SpeedUpEnemySpawn();
        SpawnEnemyWithTimer(); 
    }

    private void CountObstacleObjectInGame()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        List<GameObject> AllDeadBodies = new List<GameObject>();
        List<GameObject> AllTrashbags = new List<GameObject>();
        List<GameObject> AllBunrtFoods = new List<GameObject>();

        foreach (var item in allObjects) 
        {
            if (item.name.Contains("(Dead)"))
            {
                AllDeadBodies.Add(item);
            }

            if (item.name.Contains("Trashbag"))
            { 
                AllTrashbags.Add(item);
            }

            if (item.GetComponent<ItemSystem>() != null)
            {
                if (item.GetComponent<ItemSystem>().isBurned)
                { 
                    AllBunrtFoods.Add(item); 
                }
            }
        }

        CountDeadBody = AllDeadBodies.Count;
        CountTrashbag = AllTrashbags.Count;
        CountBurntFood = AllBunrtFoods.Count;

        CountTotalObstacleObject = CountBurntFood + CountDeadBody + CountTrashbag;
    }

    void SpeedUpEnemySpawn() //ChargeReadyTime will decrease every 3 second;
    {
        if (currentSpawnedEnemy == MaxSpawnedEnemy)
        return;


        spawnSpeedTimer += Time.deltaTime;
        if (spawnSpeedTimer >= 3f)
        {
            ChargeReadyTime = ChargeReadyTime - CountDeadBody;

            ChargeReadyTime = ChargeReadyTime - (CountTrashbag * 5);

            ChargeReadyTime = ChargeReadyTime - (CountBurntFood * 2);

            spawnSpeedTimer = 0f;
        }
    }

    void SpawnEnemyWithTimer()
    {
        if (currentSpawnedEnemy >= MaxSpawnedEnemy) { EnemyRadiusbar.gameObject.SetActive(false); return; }
        
        EnemyRadiusbar.gameObject.SetActive(!stopAndHideUiBar);
        if (stopAndHideUiBar) return; //stop the timer when dumpster's jiggle is happening

        ChargeTimer += Time.deltaTime;

        if (ChargeTimer >= ChargeReadyTime)
        {
            //Debug.Log("Charge Completed");

            //Debug.Log("Let's spawn enemy");
            currentSpawnedEnemy++;

            try
            {
                spawner = Spawners[Random.Range(0, Spawners.Count)]; // Randomly select a spawner from the list
            }
            catch (ArgumentOutOfRangeException)
            {
                Debug.LogWarning("There is no index in Spawners arrary");
            }

            SpawnEnemy(spawner);

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

    private void SpawnEnemy(Transform targetSpawner)
    {
        if (EnemySpawnEffect != null)
        {
            stopAndHideUiBar = true;
            Jiggle dumpsterJiggle = dumpster.gameObject.GetComponents<Jiggle>()[1]; //jiggle the dumpster with jiggle[1] when enemy spawn

            if (stopAndHideUiBar == true)
            {
                StartCoroutine(WaitFor(dumpsterJiggle.jiggleInterval));
                dumpsterJiggle.StartJiggle();
            }

            GameObject enemyeffect;

            if (EnemySpawnEffectPos != null)
            { enemyeffect = Instantiate(EnemySpawnEffect, EnemySpawnEffectPos.position, EnemySpawnEffectPos.rotation); }
            else
            { enemyeffect = Instantiate(EnemySpawnEffect, dumpsterPos.position, dumpsterPos.rotation); }

            EnemyEffect enemyEffectMoveTowards = enemyeffect.GetComponent<EnemyEffect>();
            enemyEffectMoveTowards.Target = targetSpawner;
        }
        else if (EnemySpawnEffect == null)
        {
            GameObject enemy = Instantiate(EnemyForCurrentWeather, targetSpawner.position, targetSpawner.rotation);//Spawn enemy here
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

    private IEnumerator WaitFor(float dumpsterTime)
    {
        yield return new WaitForSeconds(dumpsterTime + 0.5f);
        stopAndHideUiBar = false;
    }
}


