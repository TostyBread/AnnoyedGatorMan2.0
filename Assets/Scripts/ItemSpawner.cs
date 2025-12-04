using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Tracking Options")]
    [Tooltip("Check it if you need to track Child object instead of the prefab root")]
    public bool trackChild = false;      // After respawn, reference a child inside the prefab

    [Header("Tracked Object")]
    public GameObject trackedObject;     // The object you keep track of (runtime instance)

    [Header("Child Settings")]
    [Tooltip("Name of the child object to track, if 'trackChild' is enabled (can be nested)")]
    public string childName = "";        // Only used if trackChild = true

    [Header("Prefab To Spawn")]
    public GameObject prefabToSpawn;     // This is the template that gets instantiated

    [Header("Spawn Settings")]
    public Transform spawnPoint;         // Spawn location
    public float respawnDelay = 3f;      // Respawn timer

    private float timer = 0f;
    private bool timerRunning = false;

    private void Update()
    {
        if (trackedObject == null)
            StartTimer();

        TickTimer();
    }

    private void StartTimer()
    {
        if (!timerRunning)
        {
            timerRunning = true;
            timer = respawnDelay;
        }
    }

    private void TickTimer()
    {
        if (!timerRunning) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timerRunning = false;
            RestoreReferenceAndSpawn();
        }
    }

    private void RestoreReferenceAndSpawn()
    {
        if (prefabToSpawn == null)
        {
            Debug.LogWarning("ItemSpawner: prefabToSpawn is not assigned.");
            return;
        }

        // STEP 1 — Determine what inside the prefab is used as 'reference'
        // This is only used before instantiation for validation; we'll set trackedObject
        // to the spawned instance (or spawned child) after Instantiate.
        Transform referenceInPrefab = null;

        if (trackChild)
        {
            if (string.IsNullOrEmpty(childName))
            {
                Debug.LogWarning("ItemSpawner: 'childName' is empty but 'trackChild' is enabled.");
                return;
            }

            // Robust search (recursive) for a child inside the prefab (works for nested children)
            Transform[] all = prefabToSpawn.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name == childName)
                {
                    referenceInPrefab = all[i];
                    break;
                }
            }

            if (referenceInPrefab == null)
            {
                Debug.LogWarning($"ItemSpawner: Child '{childName}' not found inside prefab '{prefabToSpawn.name}'.");
                return;
            }
        }
        else
        {
            // tracking the prefab root as the reference
            referenceInPrefab = prefabToSpawn.transform;
        }

        // Optionally assign trackedObject to the prefab asset's transform (not usually useful at runtime).
        //trackedObject = referenceInPrefab.gameObject; // <-- commented out: we prefer runtime instance below

        // STEP 2 — Spawn the prefab
        if (spawnPoint == null)
        {
            Debug.LogWarning("ItemSpawner: spawnPoint not assigned.");
            return;
        }

        GameObject spawned = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);

        // STEP 3 — Hook trackedObject to the spawned instance:
        if (trackChild)
        {
            // find the matching child inside the spawned instance and assign trackedObject to it
            Transform[] spawnedChildren = spawned.GetComponentsInChildren<Transform>(true);
            Transform spawnedChild = null;
            for (int i = 0; i < spawnedChildren.Length; i++)
            {
                if (spawnedChildren[i].name == childName)
                {
                    spawnedChild = spawnedChildren[i];
                    break;
                }
            }

            if (spawnedChild != null)
            {
                trackedObject = spawnedChild.gameObject;
            }
            else
            {
                // Fallback: assign the spawned root if child unexpectedly missing
                Debug.LogWarning($"ItemSpawner: Spawned prefab does not contain child '{childName}' (unexpected). Assigning spawned root instead.");
                trackedObject = spawned;
            }
        }
        else
        {
            // track the spawned root
            trackedObject = spawned;
        }
    }
}