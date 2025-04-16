// File: NPCSpawner.cs
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
    public GameObject idLabelPrefab;

    [Header("Line Targets")]
    public LineWaypointSet[] lines;
    [Header("Exit Paths")]
    public LineWaypointSet[] exitPaths;

    private int nextCustomerId = 1;

    public GameObject SpawnNPC()
    {
        int lineIndex = Random.Range(0, lines.Length);
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
        npcBehavior.SetCustomerId(nextCustomerId, idLabelPrefab);
        npcBehavior.SetExitPath(exitPaths[lineIndex].waypoints);

        nextCustomerId++;

        return npc;
    }
}