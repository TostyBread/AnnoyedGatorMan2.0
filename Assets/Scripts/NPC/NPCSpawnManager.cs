using UnityEngine;
using System.Collections.Generic;

public class NPCSpawnManager : MonoBehaviour
{
    public NPCSpawner spawner;
    public float spawnInterval = 5f;
    public int maxNPCs = 5;

    private float timer;
    private List<GameObject> activeNPCs = new List<GameObject>();

    void Update()
    {
        timer += Time.deltaTime;

        // Remove null entries (cleaned-up NPCs)
        activeNPCs.RemoveAll(npc => npc == null);

        if (timer >= spawnInterval && activeNPCs.Count < maxNPCs)
        {
            timer = 0f;
            GameObject newNPC = spawner.SpawnNPC();
            if (newNPC != null)
                activeNPCs.Add(newNPC);
        }
    }
}