using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PestSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject[] pests;
    public GameObject[] spawnPos;
    public float minSpawnInterval;
    public float maxSpawnInterval;
    public int maxPestCount = 5; // Maximum number of pests allowed at a time

    [Header("References")]
    public Sanity sanity;

    private bool isSpawning = false;
    private List<GameObject> spawnedPests = new List<GameObject>();

    void Update()
    {
        if (sanity == null) return;

        // Remove destroyed pests from the list
        spawnedPests.RemoveAll(pest => pest == null);

        if (sanity.RemainSanity <= 0 && spawnedPests.Count < maxPestCount)
        {
            if (!isSpawning) StartCoroutine(RepeatSpawnPest(minSpawnInterval, maxSpawnInterval));
        }
    }

    private void SpawnPest()
    {
        if (pests == null || pests.Length == 0)
        {
            Debug.LogError("No pest prefabs assigned.");
            return;
        }
        if (spawnPos == null || spawnPos.Length == 0)
        {
            Debug.LogError("Spawn positions not assigned.");
            return;
        }
        if (spawnedPests.Count >= maxPestCount)
        {
            Debug.Log("Max pest count reached, stopping spawn.");
            return;
        }

        int pestRandomIndex = Random.Range(0, pests.Length);
        int spawnPosRandomIndex = Random.Range(0, spawnPos.Length);

        GameObject spawnedPest = Instantiate(pests[pestRandomIndex], spawnPos[spawnPosRandomIndex].transform.position, spawnPos[spawnPosRandomIndex].transform.rotation);
        spawnedPests.Add(spawnedPest);
        Debug.Log("Pest spawned: " + spawnedPest.name);
    }

    IEnumerator RepeatSpawnPest(float minInterval, float maxInterval)
    {
        isSpawning = true;
        yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
        if (spawnedPests.Count < maxPestCount)
        {
            SpawnPest();
        }
        isSpawning = false;
    }
}
