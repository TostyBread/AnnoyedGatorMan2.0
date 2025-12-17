using System.Collections.Generic;
using UnityEngine;

public class NPCSpawnManager : MonoBehaviour
{
    [Header("Spawner Settings")]
    public NPCSpawner spawner;
    public float baseInterval = 3f;
    public float maxInterval = 8f;
    public int maxNPCs = 5;
    public int slowDownThreshold = 2; // When to start increasing interval
    public float intervalGrowthPerNPC = 0.5f; // How much to increase per extra NPC after threshold

    private float timer;
    private int nextCustomerId = 1;
    private readonly List<GameObject> activeNPCs = new List<GameObject>();
    private readonly Dictionary<GameObject, int> npcLineMap = new Dictionary<GameObject, int>();
    private bool[] lineOccupied;
    private DialogueManager dialogueManager;

    void Start()
    {
        lineOccupied = new bool[spawner.lines.Length];
        dialogueManager = FindObjectOfType<DialogueManager>();
    }

    void Update()
    {
        if (dialogueManager != null && dialogueManager.isDialogueActive) return;

        timer += Time.deltaTime;

        // Clean up null NPCs and free lines
        for (int i = activeNPCs.Count - 1; i >= 0; i--)
        {
            if (activeNPCs[i] == null)
            {
                if (npcLineMap.TryGetValue(activeNPCs[i], out int freedLine))
                {
                    lineOccupied[freedLine] = false;
                    npcLineMap.Remove(activeNPCs[i]);
                }
                activeNPCs.RemoveAt(i);
            }
        }

        float currentInterval = CalculateSpawnInterval();
        if (timer >= currentInterval && activeNPCs.Count < maxNPCs)
        {
            timer = 0f;

            int chosenLine = GetAvailableLine();
            GameObject newNPC = spawner.SpawnNPCOnLine(chosenLine);
            if (newNPC != null)
            {
                activeNPCs.Add(newNPC);
                npcLineMap[newNPC] = chosenLine;
                lineOccupied[chosenLine] = true;

                if (newNPC.TryGetComponent(out NPCBehavior npcBehavior))
                {
                    npcBehavior.SetCustomerId(nextCustomerId++);
                }
            }
        }
    }

    private int GetAvailableLine()
    {
        for (int i = 0; i < lineOccupied.Length; i++)
        {
            if (!lineOccupied[i])
                return i;
        }

        // All full, fallback to random line
        return Random.Range(0, lineOccupied.Length);
    }

    private float CalculateSpawnInterval()
    {
        if (activeNPCs.Count <= slowDownThreshold)
            return baseInterval;

        float fillRatio = (float)activeNPCs.Count / maxNPCs;
        return Mathf.Lerp(baseInterval, maxInterval, fillRatio);
    }
}