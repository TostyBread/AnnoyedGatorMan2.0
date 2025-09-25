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
    private Collider2D npcCollider;
    private NPCAngerBehavior angerBehavior;

    [Header("Score")]
    private ScoreManager scoreManager;

    private Sanity sanity;
    private ParticleManager particleManager;

    private GameObject menuPrefab;
    private GameObject platePrefab;

    void Awake()
    {
        angerBehavior = GetComponent<NPCAngerBehavior>();
        rb = GetComponent<Rigidbody2D>();
        npcCollider = GetComponent<Collider2D>();
        particleManager = GetComponent<ParticleManager>();

        scoreManager = FindObjectOfType<ScoreManager>();
        sanity = FindObjectOfType<Sanity>();
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        if (returningToArrivedPoint)
        {
            Vector2 newPosition = Vector2.MoveTowards(rb.position, arrivedPosition, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);

            if (Vector2.Distance(rb.position, arrivedPosition) < 0.05f)
            {
                returningToArrivedPoint = false;
                rb.velocity = Vector2.zero;
            }
            return;
        }

        if (angerBehavior != null && angerBehavior.IsAngry && state != NPCState.Arrived) return;

        switch (state)
        {
            case NPCState.Approaching:
            case NPCState.Leaving:
            case NPCState.Frustrated:
            case NPCState.Escaping:
                MoveAlongPath(state == NPCState.Approaching ? waypoints : exitWaypoints);
                break;
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
        var label = GetComponentInChildren<LabelDisplay>();
        if (label != null) label.SetLabelFromId(customerId);
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
            GetComponent<NPCPatience>()?.StartPatience();
            patienceAlreadyStarted = true;
        }
    }

    public void SpawnMenuAndPlate()
    {
        if (!menuAlreadySpawned && menuPrefab)
        {
            MenuManager.Instance?.SpawnMenuForNPC(this, menuPrefab);
            menuAlreadySpawned = true;
        }

        if (!plateAlreadySpawned && platePrefab)
        {
            PlateManager.Instance?.SpawnPlateForNPC(this, platePrefab);
            plateAlreadySpawned = true;
        }

        if (attachedMenu != null && attachedPlate != null)
        {
            PlateSystem plateSys = attachedPlate.GetComponent<PlateSystem>();
            PlateMenuDisplay menuDisp = attachedMenu.GetComponent<PlateMenuDisplay>();
            if (plateSys != null && menuDisp != null)
            {
                plateSys.menuDisplay = menuDisp;
            }
        }
    }

    public void ForceEscape()
    {
        if (state == NPCState.Escaping) return;

        GetComponent<NPCPatience>()?.StopPatience();

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
            MenuManager.Instance?.RemoveMenuForNPC(this);
        }

        if (attachedPlate != null)
        {
            attachedPlate.GetComponent<PlateSystem>()?.SetOwnerActive(false);
            attachedPlate = null;
        }

        PlateManager.Instance?.FreeSpawnPoint(this);
    }

    public bool TryAcceptPlate(PlateSystem plate)
    {
        if (plate == null || !plate.isReadyToServe) return false;

        var label = plate.GetComponentInChildren<LabelDisplay>();
        if (label != null && label.labelText == customerId.ToString())
        {
            label.DisableLabel();

            Transform plateRoot = plate.rootPlateObject != null ? plate.rootPlateObject : plate.transform;
            plateRoot.SetParent(plateReceiveAnchor ?? transform);
            plateRoot.localPosition = plateReceiveAnchor ? Vector3.zero : Vector3.zero;

            npcCollider.enabled = false;
            Rigidbody2D plateRb = plateRoot.GetComponent<Rigidbody2D>();
            if (plateRb) plateRb.bodyType = RigidbodyType2D.Kinematic;
            AudioManager.Instance.PlaySound("yes", transform.position);

            state = NPCState.Leaving;
            currentWaypointIndex = 0;

            scoreManager?.AddScore(plate.plateScore);
            if (sanity != null) sanity.RemainSanity += sanity.MaxSanity;
            particleManager?.SpawnParticleOnce();
            hasAcceptedPlate = true;

            if (angerBehavior != null && angerBehavior.IsAngry)
            {
                GetComponentInChildren<NPCAnimationController>()?.SetIsAngry(false);
            }

            if (attachedMenu != null)
            {
                attachedMenu = null;
                MenuManager.Instance?.RemoveMenuForNPC(this);
            }
            GetComponent<NPCPatience>()?.StopPatience();

            PlateManager.Instance?.FreeSpawnPoint(this);
            return true;
        }
        return false;
    }

    public void FrustratedLeaving()
    {
        if (state == NPCState.Frustrated) return;

        if (!hasAcceptedPlate && angerBehavior != null)
        {
            if (Random.value < angerBehavior.angerChanceOnHit)
            {
                angerBehavior.TriggerAngerMode(null);
            }
        }

        state = NPCState.Frustrated;
        currentWaypointIndex = 0;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        if (attachedMenu != null)
        {
            attachedMenu = null;
            MenuManager.Instance?.RemoveMenuForNPC(this);
        }

        if (attachedPlate != null)
        {
            attachedPlate.GetComponent<PlateSystem>()?.SetOwnerActive(false);
            attachedPlate = null;
        }

        GetComponent<NPCPatience>()?.StopPatience();

        if (angerBehavior == null || !angerBehavior.IsAngry)
        {
            npcCollider.enabled = false;
        }

        PlateManager.Instance?.FreeSpawnPoint(this);
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

        Vector2 targetPosition = path[currentWaypointIndex];
        Vector2 direction = (targetPosition - rb.position).normalized;

        if (state == NPCState.Approaching)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(rb.position, direction, detectionRange, npcLayer);
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
        Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, currentSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);

        if (Vector2.Distance(rb.position, targetPosition) < 0.05f)
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
            angerBehavior?.RegisterHit(collision.gameObject);

            if (attachedMenu != null)
            {
                attachedMenu = null;
                MenuManager.Instance?.RemoveMenuForNPC(this);
            }
            ForceEscape();

            if (angerBehavior == null || !angerBehavior.IsAngry)
            {
                npcCollider.enabled = false;
                AudioManager.Instance.PlaySound("scream", transform.position);
            }
        }

        if (state == NPCState.Arrived)
        {
            returningToArrivedPoint = true;
            return;
        }
    }
}