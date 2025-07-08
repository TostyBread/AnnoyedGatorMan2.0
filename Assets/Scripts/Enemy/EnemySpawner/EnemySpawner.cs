using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;


public class EnemySpawner : MonoBehaviour
{
    public List<GameObject> Enemies = new List<GameObject>();
    public GameObject UI; 
    private Timer timer;
    private Sanity sanity;
    public List<Transform> Spawners = new List<Transform>();

    [Header("do not touch, just for Refrence")]
    [SerializeField] private float ChargeReadyTime;
    [SerializeField] private float ChargeTimer;

    bool SanityIsEmptyOnce;

    // Start is called before the first frame update
    void Start()
    {
        if (UI == null)
        {
            UI = GameObject.Find("UI");
            Debug.LogWarning("UI is missing, So " + gameObject + " will find gameObject name: UI");
        }

        timer = UI.GetComponentInChildren<Timer>();
        sanity = UI.GetComponentInChildren<Sanity>();
        ChargeReadyTime = Random.Range(10,20);

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
    }

    // Update is called once per frame
    void Update()
    {
        if (SanityIsEmptyOnce == false && sanity.RemainSanity == 0)
        {
            foreach (Transform spawner in Spawners)
            {
                GameObject enemy = Instantiate(Enemies[Random.Range(0, Enemies.Count)], spawner.position, spawner.rotation);
            }
            ChargeReadyTime = Random.Range(5, 10);
            ChargeTimer = 0;
            SanityIsEmptyOnce = true;
        }

        Timer();
    }

    void Timer()
    {
        ChargeTimer += Time.deltaTime;


        if (ChargeTimer >= ChargeReadyTime)
        {
            Debug.Log("Charge Completed");

            Debug.Log("Let's spawn enemy");
            Transform spawner = Spawners[Random.Range(0, Spawners.Count)];
            GameObject enemy = Instantiate(Enemies[Random.Range(0, Enemies.Count)], spawner.position, spawner.rotation);

            ChargeReadyTime = Random.Range(5, 10);

            if (sanity.RemainSanity <= 0) //If sanity is empty...
            {
                if (SanityIsEmptyOnce == false) // All spawner will spawn enemy once
                {
                    foreach (Transform spawnPos in Spawners)
                    {
                        GameObject enemies = Instantiate(Enemies[Random.Range(0, Enemies.Count)], spawnPos.position, spawnPos.rotation);
                    }
                    SanityIsEmptyOnce = true;
                }

                ChargeReadyTime /= 2;  // enemy spawn speed is doubled
            }

            ChargeTimer = 0;
        }

    }
}
