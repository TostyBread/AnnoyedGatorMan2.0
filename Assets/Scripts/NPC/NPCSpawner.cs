using UnityEngine;

[System.Serializable]
public class LineWaypointSet
{
    public string name;
    public Transform[] waypoints;
}

public class NPCSpawner : MonoBehaviour
{
    public GameObject npcPrefab;
    public GameObject platePrefab;
    public GameObject menuPrefab;

    [Header("Line Targets")]
    public LineWaypointSet[] lines;

    public GameObject SpawnNPC()
    {
        int index = Random.Range(0, lines.Length);
        Transform[] selectedPath = lines[index].waypoints;

        GameObject npc = Instantiate(npcPrefab, transform.position, Quaternion.identity);

        NPCBehavior npcBehavior = npc.GetComponent<NPCBehavior>();
        Vector3[] path = new Vector3[selectedPath.Length];
        for (int i = 0; i < selectedPath.Length; i++)
            path[i] = selectedPath[i].position;

        npcBehavior.SetWaypoints(path);
        npcBehavior.SetMenuAndPlatePrefabs(menuPrefab, platePrefab);

        return npc;
    }
}