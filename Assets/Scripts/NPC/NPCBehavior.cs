using UnityEngine;

public class NPCBehavior : MonoBehaviour
{
    public enum NPCState { Approaching, Arrived, Leaving, Escaping, Frustrated }

    public int customerType; // assign a number for MenuUISetup to use
    public float moveSpeed = 2f;
    public float forceEscapeThreshold = 5f;
    public float escapeSpeedMultiplier = 2f;
    public Transform plateReceiveAnchor;
    public bool hasAcceptedPlate = false;

    [Header("NPC Detection")]
    public float detectionRange = 1.0f;
    public LayerMask npcLayer;

    [Header("Plate Detection")]
    [Tooltip("How often to scan for plates in range (in seconds)")]
    public float plateDetectionInterval = 0.2f;
    [Tooltip("Range to detect plates")]
    public float plateDetectionRange = 1.5f;
    [Tooltip("Layer mask for plate detection")]
    public LayerMask plateLayer = -1;

    [Header("Collider References")]
    [Tooltip("Assign the trigger collider for plate detection")]
    public Collider2D triggerCollider;
    [Tooltip("Assign the regular collider for physics collisions")]
    public Collider2D physicsCollider;

    [Header("Performance Settings")]
    [Tooltip("Distance threshold for waypoint completion (smaller = more precise, larger = more performance)")]
    public float waypointThreshold = 0.05f;
    [Tooltip("Distance threshold for return to position (smaller = more precise, larger = more performance)")]
    public float returnThreshold = 0.05f;

    private Vector3[] waypoints;
    private Vector3[] exitWaypoints;
    private int currentWaypointIndex = 0;

    private GameObject attachedPlate;
    private GameObject attachedMenu;
    public Rigidbody2D rb;
    private Vector3 arrivedPosition;
    private bool returningToArrivedPoint = false;
    private bool patienceAlreadyStarted = false;
    private bool menuAlreadySpawned = false;
    private bool plateAlreadySpawned = false;

    public int customerId { get; private set; }
    private NPCState state = NPCState.Approaching;
    private NPCAngerBehavior angerBehavior;

    // Cached components for performance
    private NPCPatience npcPatience;
    private NPCAnimationController npcAnimController;
    private LabelDisplay labelDisplay;

    [Header("Score")]
    private ScoreManager scoreManager;
    private Sanity sanity;
    private ParticleManager particleManager;

    private GameObject menuPrefab;
    private GameObject platePrefab;

    // Performance optimization: Cache frequently used values
    private Vector2 cachedRbPosition;
    private Vector3 cachedTargetPosition;
    private float distanceToTargetSqr; // Use squared distance for better performance

    // Cache static references to avoid repeated FindObjectOfType calls
    private static MenuManager menuManagerInstance;
    private static PlateManager plateManagerInstance;
    private static AudioManager audioManagerInstance;

    // Plate detection variables
    private float lastPlateDetectionTime;

    void Awake()
    {
        // Cache all components at startup
        angerBehavior = GetComponent<NPCAngerBehavior>();
        rb = GetComponent<Rigidbody2D>();
        
        // Auto-assign colliders if not manually set
        AutoAssignColliders();
        
        particleManager = GetComponent<ParticleManager>();
        npcPatience = GetComponent<NPCPatience>();
        npcAnimController = GetComponentInChildren<NPCAnimationController>();
        labelDisplay = GetComponentInChildren<LabelDisplay>();

        // Cache singleton instances (with null check for first NPC)
        if (scoreManager == null)
            scoreManager = FindObjectOfType<ScoreManager>();
        if (sanity == null)
            sanity = FindObjectOfType<Sanity>();

        // Cache static manager references
        if (menuManagerInstance == null)
            menuManagerInstance = MenuManager.Instance;
        if (plateManagerInstance == null)
            plateManagerInstance = PlateManager.Instance;
        if (audioManagerInstance == null)
            audioManagerInstance = AudioManager.Instance;
    }

    private void AutoAssignColliders()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();
        
        foreach (Collider2D col in colliders)
        {
            if (col.isTrigger && triggerCollider == null)
            {
                triggerCollider = col;
            }
            else if (!col.isTrigger && physicsCollider == null)
            {
                physicsCollider = col;
            }
        }

        // Fallback: if only one collider exists, use it for both (but log a warning)
        if (colliders.Length == 1)
        {
            if (triggerCollider == null) triggerCollider = colliders[0];
            if (physicsCollider == null) physicsCollider = colliders[0];
            Debug.LogWarning($"NPC {gameObject.name} only has one collider. Consider adding separate trigger and physics colliders for better functionality.");
        }

        // Ensure we have valid references
        if (triggerCollider == null || physicsCollider == null)
        {
            Debug.LogError($"NPC {gameObject.name} is missing required colliders. Please assign trigger and physics colliders manually.");
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // Cache rb.position to avoid multiple property calls
        cachedRbPosition = rb.position;

        if (returningToArrivedPoint)
        {
            HandleReturnToArrivedPoint();
            return;
        }

        // Allow movement during anger mode - removed the anger check that was preventing movement
        // if (angerBehavior != null && angerBehavior.IsAngry && state != NPCState.Arrived) return;

        switch (state)
        {
            case NPCState.Approaching:
            case NPCState.Leaving:
            case NPCState.Frustrated:
            case NPCState.Escaping:
                MoveAlongPath(state == NPCState.Approaching ? waypoints : exitWaypoints);
                break;
            case NPCState.Arrived:
                // Actively scan for plates when arrived
                CheckForPlatesInRange();
                break;
        }
    }

    private void CheckForPlatesInRange()
    {
        if (hasAcceptedPlate) return;
        if (Time.time - lastPlateDetectionTime < plateDetectionInterval) return;

        lastPlateDetectionTime = Time.time;

        // Use the trigger collider's bounds for more accurate detection
        Vector2 detectionCenter = triggerCollider != null ? triggerCollider.bounds.center : transform.position;
        
        // Use OverlapCircleAll to find all colliders in range
        Collider2D[] collidersInRange = Physics2D.OverlapCircleAll(detectionCenter, plateDetectionRange, plateLayer);

        foreach (Collider2D collider in collidersInRange)
        {
            // Check for PlateSystem component in the collider or its children
            PlateSystem plate = collider.GetComponent<PlateSystem>();
            if (plate == null)
                plate = collider.GetComponentInChildren<PlateSystem>();

            if (plate != null && TryAcceptPlate(plate))
            {
                break; // Stop checking once we've accepted a plate
            }
        }
    }

    private void HandleReturnToArrivedPoint()
    {
        Vector2 newPosition = Vector2.MoveTowards(cachedRbPosition, arrivedPosition, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);

        // Use squared distance for better performance
        float distanceSqr = (cachedRbPosition - (Vector2)arrivedPosition).sqrMagnitude;
        if (distanceSqr < returnThreshold * returnThreshold)
        {
            returningToArrivedPoint = false;
            rb.velocity = Vector2.zero;
        }
    }

    public void SetWaypoints(Vector3[] path)
    {
        waypoints = path;
        currentWaypointIndex = 0;
        state = NPCState.Approaching;
    }

    public void SetExitPath(Transform[] exitPath)
    {
        exitWaypoints = new Vector3[exitPath.Length];
        for (int i = 0; i < exitPath.Length; i++)
            exitWaypoints[i] = exitPath[i].position;
    }

    public void SetCustomerId(int id)
    {
        customerId = id;
        // Use cached component reference
        labelDisplay?.SetLabelFromId(customerId);
    }

    public void SetMenuAndPlatePrefabs(GameObject menu, GameObject plate)
    {
        menuPrefab = menu;
        platePrefab = plate;
    }

    public void AttachMenu(GameObject menu)
    {
        attachedMenu = menu;
    }

    public void AttachPlate(GameObject plate)
    {
        attachedPlate = plate;
    }

    public void TriggerPatience()
    {
        if (state == NPCState.Arrived && !patienceAlreadyStarted)
        {
            // Use cached component reference
            npcPatience?.StartPatience();
            patienceAlreadyStarted = true;
        }
    }

    public void SpawnMenuAndPlate()
    {
        if (!menuAlreadySpawned && menuPrefab)
        {
            menuManagerInstance?.SpawnMenuForNPC(this, menuPrefab);
            menuAlreadySpawned = true;
        }

        if (!plateAlreadySpawned && platePrefab)
        {
            plateManagerInstance?.SpawnPlateForNPC(this, platePrefab);
            plateAlreadySpawned = true;
        }

        if (attachedMenu != null && attachedPlate != null)
        {
            // Cache components to avoid repeated GetComponent calls
            if (attachedPlate.TryGetComponent<PlateSystem>(out var plateSys) && 
                attachedMenu.TryGetComponent<PlateMenuDisplay>(out var menuDisp))
            {
                plateSys.menuDisplay = menuDisp;
            }
        }
    }

    public void ForceEscape()
    {
        if (state == NPCState.Escaping) return;

        // Use cached component reference
        npcPatience?.StopPatience();

        if (waypoints != null && waypoints.Length > 0 && exitWaypoints != null && exitWaypoints.Length > 0)
        {
            Vector3 lastExit = exitWaypoints[exitWaypoints.Length - 1];
            exitWaypoints = new Vector3[] { lastExit };
        }

        state = NPCState.Escaping;
        currentWaypointIndex = 0;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        if (attachedMenu != null)
        {
            attachedMenu = null;
            menuManagerInstance?.RemoveMenuForNPC(this);
        }

        if (attachedPlate != null)
        {
            if (attachedPlate.TryGetComponent<PlateSystem>(out var plateSystem))
                plateSystem.SetOwnerActive(false);
            attachedPlate = null;
        }

        plateManagerInstance?.FreeSpawnPoint(this);
    }

    public bool TryAcceptPlate(PlateSystem plate)
    {
        if (plate == null || !plate.isReadyToServe) return false;

        // Check if the plate is currently being held by a player
        if (plate.IsHeld) return false;

        LabelDisplay label = plate.GetComponentInChildren<LabelDisplay>();
        if (label != null && label.labelText == customerId.ToString())
        {
            label.DisableLabel();

            Transform plateRoot = plate.rootPlateObject != null ? plate.rootPlateObject : plate.transform;
            plateRoot.SetParent(plateReceiveAnchor ?? transform);
            plateRoot.localPosition = Vector3.zero;

            // Disable the physics collider, not the trigger (so we can still detect other triggers)
            if (physicsCollider != null)
                physicsCollider.enabled = false;
            
            // Use TryGetComponent for better performance
            if (plateRoot.TryGetComponent<Rigidbody2D>(out var plateRb))
                plateRb.bodyType = RigidbodyType2D.Kinematic;
                
            audioManagerInstance.PlaySound("yes", transform.position);

            state = NPCState.Leaving;
            currentWaypointIndex = 0;

            scoreManager?.AddScore(plate.plateScore);
            if (sanity != null) sanity.RemainSanity += sanity.MaxSanity;
            particleManager?.SpawnParticleOnce();
            hasAcceptedPlate = true;

            if (angerBehavior != null && angerBehavior.IsAngry)
            {
                // Use cached component reference
                npcAnimController?.SetIsAngry(false);
            }

            if (attachedMenu != null)
            {
                attachedMenu = null;
                menuManagerInstance?.RemoveMenuForNPC(this);
            }
            
            // Use cached component reference
            npcPatience?.StopPatience();
            plateManagerInstance?.FreeSpawnPoint(this);
            return true;
        }
        return false;
    }

    public void FrustratedLeaving()
    {
        if (state == NPCState.Frustrated) return;

        bool angerTriggered = false;
        
        if (!hasAcceptedPlate && angerBehavior != null)
        {
            if (Random.value < angerBehavior.angerChanceOnHit)
            {
                angerBehavior.TriggerAngerMode(null);
                angerTriggered = true;
            }
        }

        // If anger was triggered, don't immediately leave - let the anger play out first
        if (angerTriggered && angerBehavior != null && angerBehavior.IsAngry)
        {
            // Clean up the menu but don't change state to Frustrated yet
            if (attachedMenu != null)
            {
                attachedMenu = null;
                menuManagerInstance?.RemoveMenuForNPC(this);
            }

            if (attachedPlate != null)
            {
                if (attachedPlate.TryGetComponent<PlateSystem>(out var plateSystem))
                    plateSystem.SetOwnerActive(false);
                attachedPlate = null;
            }

            // Stop patience and free spawn point
            npcPatience?.StopPatience();
            plateManagerInstance?.FreeSpawnPoint(this);
            
            // Keep collider enabled for angry NPC interactions
            // The anger system will handle when to escape via AngerTimerRoutine or hit count
            return; // Don't proceed to frustrated leaving - let anger handle the exit
        }

        // Normal frustrated leaving behavior (when anger wasn't triggered or failed to trigger)
        state = NPCState.Frustrated;
        currentWaypointIndex = 0;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        if (attachedMenu != null)
        {
            attachedMenu = null;
            menuManagerInstance?.RemoveMenuForNPC(this);
        }

        if (attachedPlate != null)
        {
            if (attachedPlate.TryGetComponent<PlateSystem>(out var plateSystem))
                plateSystem.SetOwnerActive(false);
            attachedPlate = null;
        }

        // Use cached component reference
        npcPatience?.StopPatience();

        if (angerBehavior == null || !angerBehavior.IsAngry)
        {
            // Disable physics collider, keep trigger for consistency
            if (physicsCollider != null)
                physicsCollider.enabled = false;
        }

        plateManagerInstance?.FreeSpawnPoint(this);
    }

    private void MoveAlongPath(Vector3[] path)
    {
        if (path == null || path.Length == 0) return;

        if (currentWaypointIndex >= path.Length)
        {
            if (state == NPCState.Leaving || state == NPCState.Escaping || state == NPCState.Frustrated)
            {
                Destroy(gameObject);
            }
            else
            {
                state = NPCState.Arrived;
                arrivedPosition = waypoints[waypoints.Length - 1];
            }
            return;
        }

        // Cache target position
        cachedTargetPosition = path[currentWaypointIndex];
        Vector2 targetPosition2D = cachedTargetPosition;
        Vector2 direction = (targetPosition2D - cachedRbPosition).normalized;

        if (state == NPCState.Approaching)
        {
            // Only perform raycast when approaching to avoid collision
            RaycastHit2D[] hits = Physics2D.RaycastAll(cachedRbPosition, direction, detectionRange, npcLayer);
            foreach (var hit in hits)
            {
                if (hit.collider != null && hit.collider.gameObject != gameObject)
                {
                    rb.velocity = Vector2.zero;
                    return;
                }
            }
        }

        float currentSpeed = state == NPCState.Escaping ? moveSpeed * escapeSpeedMultiplier : moveSpeed;
        Vector2 newPosition = Vector2.MoveTowards(cachedRbPosition, targetPosition2D, currentSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);

        // Use squared distance for better performance
        distanceToTargetSqr = (cachedRbPosition - targetPosition2D).sqrMagnitude;
        if (distanceToTargetSqr < waypointThreshold * waypointThreshold)
        {
            currentWaypointIndex++;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (state != NPCState.Arrived) return;

        PlateSystem plate = collision.GetComponentInChildren<PlateSystem>();
        if (plate != null)
            TryAcceptPlate(plate);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.relativeVelocity.magnitude > forceEscapeThreshold)
        {
            // Let the anger behavior handle the hit and decide whether to escape
            angerBehavior?.RegisterHit(collision.gameObject);

            // Only clean up menu here, let anger behavior decide if escape is needed
            if (attachedMenu != null)
            {
                attachedMenu = null;
                menuManagerInstance?.RemoveMenuForNPC(this);
            }

            // Only force escape immediately if there's no anger behavior or NPC is not angry
            if (angerBehavior == null || !angerBehavior.IsAngry)
            {
                ForceEscape();
                if (physicsCollider != null)
                    physicsCollider.enabled = false;
                audioManagerInstance.PlaySound("scream", transform.position);
            }
        }

        if (state == NPCState.Arrived)
        {
            returningToArrivedPoint = true;
            return;
        }
    }

    // Debug visualization for plate detection range
    private void OnDrawGizmosSelected()
    {
        if (state == NPCState.Arrived)
        {
            Vector3 center = triggerCollider != null ? triggerCollider.bounds.center : transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(center, plateDetectionRange);
        }

        // Show collider references
        if (triggerCollider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(triggerCollider.bounds.center, triggerCollider.bounds.size);
        }

        if (physicsCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(physicsCollider.bounds.center, physicsCollider.bounds.size);
        }
    }
}