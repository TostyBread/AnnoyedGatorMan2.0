using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public GameObject npcPrefab;
    public Transform[] spawnLines;
    public GameObject platePrefab;
    public GameObject menuPrefab;

    public GameObject SpawnNPC()
    {
        int index = Random.Range(0, spawnLines.Length);
        Transform targetLine = spawnLines[index];

        GameObject npc = Instantiate(npcPrefab, transform.position, Quaternion.identity);

        NPCBehavior npcBehavior = npc.GetComponent<NPCBehavior>();
        npcBehavior.SetTarget(targetLine.position);
        npcBehavior.SetMenuAndPlatePrefabs(menuPrefab, platePrefab);

        return npc;
    }
}