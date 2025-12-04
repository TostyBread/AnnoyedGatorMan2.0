using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedUpEnemySpawnerEffect : MonoBehaviour
{
    public GameObject particle;
    private EnemySpawner enemySpawner;
    bool once;

    private float spawnSpeedTimer;
    private float timeDecreaseEverySec;

    private ItemDescriber itemDescriber;
    // Start is called before the first frame update
    void Start()
    {
        enemySpawner = FindObjectOfType<EnemySpawner>();

        timeDecreaseEverySec = enemySpawner.timeDecreaseEverySec;

        itemDescriber = GetComponent<ItemDescriber>();
    }

    // Update is called once per frame
    void Update()
    {
        spawnSpeedTimer = enemySpawner.spawnSpeedTimer;

        if (itemDescriber != null)
        {
            if (itemDescriber.currentCookingState == ItemDescriber.CookingState.Overcooked && once == false)
            {
                StartCoroutine(spawnSpeedUpEnemySpawnerEffect());
            }
        }
        else 
        {
            if (spawnSpeedTimer >= timeDecreaseEverySec && once == false)
            {
                StartCoroutine(spawnSpeedUpEnemySpawnerEffect());
            }
        }
    }

    IEnumerator spawnSpeedUpEnemySpawnerEffect()
    {
        once = true;
        GameObject enemyDieEffect = Instantiate(particle, transform.position, transform.rotation);
        enemyDieEffect.SetActive(true);
        yield return new WaitForSeconds(enemySpawner.timeDecreaseEverySec);
        once = false;
    }
}
