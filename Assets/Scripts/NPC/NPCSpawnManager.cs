using UnityEngine;
using System.Collections.Generic;

public class NPCSpawnManager : MonoBehaviour
{
    [Header("Spawner Settings")]
    public NPCSpawner spawner;
    public float spawnInterval = 5f;
    public int maxNPCs = 5;

    private float timer;
    private int nextCustomerId = 1;
    private readonly List<GameObject> activeNPCs = new List<GameObject>();

    void Update()
    {
        timer += Time.deltaTime;
        activeNPCs.RemoveAll(npc => npc == null);

        if (timer >= spawnInterval && activeNPCs.Count < maxNPCs)
        {
            timer = 0f;
            GameObject newNPC = spawner.SpawnNPC();
            if (newNPC != null)
            {
                activeNPCs.Add(newNPC);

                if (newNPC.TryGetComponent(out NPCBehavior npcBehavior))
                {
                    npcBehavior.SetCustomerId(nextCustomerId++);
                }
            }
        }
    }
}