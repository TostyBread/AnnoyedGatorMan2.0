using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDieEffect : MonoBehaviour
{
    public GameObject particle;
    private EnemySpawner enemySpawner;
    bool once;

    private float spawnSpeedTimer;
    private float timeDecreaseEverySec;
    // Start is called before the first frame update
    void Start()
    {
        enemySpawner = FindObjectOfType<EnemySpawner>();

        timeDecreaseEverySec = enemySpawner.timeDecreaseEverySec;
    }

    // Update is called once per frame
    void Update()
    {
        spawnSpeedTimer = enemySpawner.spawnSpeedTimer;

        if (spawnSpeedTimer >= timeDecreaseEverySec && once == false)
        {
            StartCoroutine(spawnEnemyDieEffect());
        }
    }

    IEnumerator spawnEnemyDieEffect()
    {
        once = true;
        GameObject enemyDieEffect = Instantiate(particle, transform.position, transform.rotation);
        yield return new WaitForSeconds(enemySpawner.timeDecreaseEverySec);

        once = false;
    }
}
