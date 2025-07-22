using UnityEngine;

[System.Serializable]
public class LineWaypointSet
{
    public string name;
    public Transform[] waypoints;
}

[System.Serializable]
public class PlateMenuSet
{
    public GameObject platePrefab;
    public GameObject menuPrefab;
}

public class NPCSpawner : MonoBehaviour
{
    public GameObject[] npcPrefabs;
    public PlateMenuSet[] plateMenuSets;

    [Header("Line Targets")]
    public LineWaypointSet[] lines;
    private bool[] lineOccupied;

    [Header("Exit Paths")]
    public LineWaypointSet[] exitPaths;

    private bool npcInSpawnArea = false;

    void Awake()
    {
        lineOccupied = new bool[lines.Length];
    }

    public void NotifyLineFreed(int lineIndex)
    {
        if (lineIndex >= 0 && lineIndex < lineOccupied.Length)
        {
            lineOccupied[lineIndex] = false;
        }
    }

    public GameObject SpawnNPCOnLine(int lineIndex)
    {
        if (npcInSpawnArea)
        {
            Debug.LogWarning("Cannot spawn NPC: spawn area is occupied.");
            return null;
        }

        int npcIndex = Random.Range(0, npcPrefabs.Length);
        int plateMenuIndex = Random.Range(0, plateMenuSets.Length);

        Transform[] selectedPath = lines[lineIndex].waypoints;
        GameObject npc = Instantiate(npcPrefabs[npcIndex], transform.position, Quaternion.identity);

        NPCBehavior npcBehavior = npc.GetComponent<NPCBehavior>();
        Vector3[] path = new Vector3[selectedPath.Length];
        for (int i = 0; i < selectedPath.Length; i++)
            path[i] = selectedPath[i].position;

        npcBehavior.SetWaypoints(path);
        npcBehavior.SetMenuAndPlatePrefabs(plateMenuSets[plateMenuIndex].menuPrefab, plateMenuSets[plateMenuIndex].platePrefab);
        npcBehavior.SetExitPath(exitPaths[lineIndex].waypoints);

        return npc;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            npcInSpawnArea = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            npcInSpawnArea = false;
        }
    }
}