using UnityEngine;

public class NPCOrderTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        NPCBehavior npc = other.GetComponent<NPCBehavior>();

        if (npc != null)
        {
            npc.SpawnMenuAndPlate();
        }
    }
}