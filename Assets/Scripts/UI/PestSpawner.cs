using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PestSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject[] pests;
    public GameObject[] spawnPos;
    public float minSpawnInterval;
    public float maxSpawnInterval;

    [Header("References")]
    public Sanity sanity;

    private bool isSpawning = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (sanity == null) return;

        // Check if sanity is depleted
        if (sanity.RemainSanity <= 0)
        {
            if (!isSpawning) StartCoroutine(RepeatSpawnPest(minSpawnInterval, maxSpawnInterval));
        }
    }

    private void SpawnPest()
    {
        // Ensure there are pests to spawn
        if (pests == null || pests.Length == 0)
        {
            Debug.LogError("No pest prefabs assigned.");
            return;
        }

        // Ensure spawn position is assigned
        if (spawnPos == null)
        {
            Debug.LogError("Spawn position is not assigned.");
            return;
        }

        // Select a random pest from the array
        int pestRandomIndex = Random.Range(0, pests.Length);
        GameObject pestToSpawn = pests[pestRandomIndex];

        // Select a random spawnPos from the array
        int spawnPosRandomIndex = Random.Range(0, spawnPos.Length);

        // Spawn the selected pest at the spawn position
        Instantiate(pestToSpawn, spawnPos[spawnPosRandomIndex].transform.position, spawnPos[spawnPosRandomIndex].transform.rotation);
        Debug.Log("Pest spawned: " + pestToSpawn.name);
    }

    IEnumerator RepeatSpawnPest(float minInterval, float maxInterval)
    {
        isSpawning = true;
        yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
        SpawnPest();
        isSpawning = false;
    }
}
