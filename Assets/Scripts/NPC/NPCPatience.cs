using UnityEngine;

[RequireComponent(typeof(NPCBehavior))]
public class NPCPatience : MonoBehaviour
{
    [Tooltip("Total patience time (seconds) before NPC gets frustrated.")]
    public float patienceDuration = 10f;
    public PatienceBarController patienceBar;

    private float currentPatience;
    private bool patienceRunning = false;

    private NPCBehavior npcBehavior;

    void Awake()
    {
        npcBehavior = GetComponent<NPCBehavior>();
        ResetPatience();
        if (patienceBar != null)
            patienceBar.Show(false);
    }

    void Update()
    {
        if (!patienceRunning || npcBehavior == null) return;

        currentPatience -= Time.deltaTime;

        if (patienceBar != null)
            patienceBar.SetPatience(currentPatience, patienceDuration);

        if (currentPatience <= 0f)
        {
            patienceRunning = false;
            if (patienceBar != null)
                patienceBar.Show(false);
            npcBehavior.FrustratedLeaving();
        }
    }

    public void StartPatience()
    {
        if (npcBehavior == null) return;

        ResetPatience();
        patienceRunning = true;
        if (patienceBar != null)
            patienceBar.Show(true);
    }

    public void StopPatience()
    {
        patienceRunning = false;
        if (patienceBar != null)
            patienceBar.Show(false);
    }

    public void ResetPatience()
    {
        currentPatience = patienceDuration;
        if (patienceBar != null)
            patienceBar.SetPatience(currentPatience, patienceDuration);
    }

    public bool IsPatienceRunning()
    {
        return patienceRunning;
    }
}