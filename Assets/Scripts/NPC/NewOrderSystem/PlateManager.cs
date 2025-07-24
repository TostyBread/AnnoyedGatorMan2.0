using UnityEngine;
using System.Collections.Generic;

public class PlateManager : MonoBehaviour
{
    public static PlateManager Instance { get; private set; }

    [SerializeField] private Transform[] plateSpawnPoints;
    private Dictionary<int, NPCBehavior> spawnOccupancy = new();

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    public void SpawnPlateForNPC(NPCBehavior npc, GameObject platePrefab)
    {
        if (platePrefab == null || npc == null || plateSpawnPoints.Length == 0) return;

        int index = GetFreeSpawnPointIndex();
        if (index == -1)
        {
            Debug.LogWarning("No free plate spawn points available.");
            return;
        }

        Transform spawnPoint = plateSpawnPoints[index];
        GameObject plate = Instantiate(platePrefab, spawnPoint.position, Quaternion.identity);

        var label = plate.GetComponentInChildren<LabelDisplay>();
        if (label != null) label.SetLabelFromId(npc.customerId);

        npc.AttachPlate(plate);
        spawnOccupancy[index] = npc;
    }

    public void SpawnPlateAtIndex(NPCBehavior npc, GameObject platePrefab, int index)
    {
        if (platePrefab == null || npc == null || plateSpawnPoints.Length == 0) return;
        if (index < 0 || index >= plateSpawnPoints.Length) return;
        if (spawnOccupancy.ContainsKey(index)) return;

        Transform spawnPoint = plateSpawnPoints[index];
        GameObject plate = Instantiate(platePrefab, spawnPoint.position, Quaternion.identity);

        var label = plate.GetComponentInChildren<LabelDisplay>();
        if (label != null) label.SetLabelFromId(npc.customerId);

        npc.AttachPlate(plate);
        spawnOccupancy[index] = npc;
    }

    public void FreeSpawnPoint(NPCBehavior npc)
    {
        foreach (var pair in spawnOccupancy)
        {
            if (pair.Value == npc)
            {
                spawnOccupancy.Remove(pair.Key);
                break;
            }
        }
    }

    private int GetFreeSpawnPointIndex()
    {
        for (int i = 0; i < plateSpawnPoints.Length; i++)
        {
            if (!spawnOccupancy.ContainsKey(i))
                return i;
        }
        return -1;
    }
}