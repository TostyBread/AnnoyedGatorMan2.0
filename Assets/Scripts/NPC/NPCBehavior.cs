using UnityEngine;

public class NPCBehavior : MonoBehaviour
{
    public enum NPCState { Approaching, Arrived, Leaving, Escaping, Frustrated }

    public float moveSpeed = 2f;
    public float forceEscapeThreshold = 5f;
    public float escapeSpeedMultiplier = 2f;
    public Transform menuSpawnPoint;
    public Transform plateSpawnPoint;
    public Transform plateReceiveAnchor;

    [Header("NPC Detection")]
    public float detectionRange = 1.0f;
    public LayerMask npcLayer;

    private Vector3[] waypoints;
    private Vector3[] exitWaypoints;
    private int currentWaypointIndex = 0;

    private GameObject menuPrefab;
    private GameObject platePrefab;
    private GameObject attachedPlate;
    private GameObject attachedMenu;
    private Rigidbody2D rb;
    private Vector3 arrivedPosition;
    private bool returningToArrivedPoint = false;
    private bool menuAlreadySpawned = false;
    private bool plateAlreadySpawned = false;
    private bool patienceAlreadyStarted = false;

    public int customerId { get; private set; }
    private NPCState state = NPCState.Approaching;
    private Collider2D npcCollider;

    [Header("Score")]
    public int score;
    private ScoreManager scoreManager;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        npcCollider = GetComponent<Collider2D>(); // Cache collider
        scoreManager = FindObjectOfType<ScoreManager>();
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        if (returningToArrivedPoint)
        {
            float currentSpeed = moveSpeed;
            Vector2 newPosition = Vector2.MoveTowards(rb.position, arrivedPosition, currentSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);

            if (Vector2.Distance(rb.position, arrivedPosition) < 0.05f)
            {
                returningToArrivedPoint = false;
                rb.velocity = Vector2.zero;
            }

            return; // Skip other states while returning
        }

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

    public void SetMenuAndPlatePrefabs(GameObject menu, GameObject plate)
    {
        menuPrefab = menu;
        platePrefab = plate;
    }

    public void SetCustomerId(int id)
    {
        customerId = id;
        var label = GetComponentInChildren<LabelDisplay>();
        if (label != null) label.SetLabelFromId(customerId);
    }

    public void SpawnMenuAndPlate()
    {
        if (!menuAlreadySpawned)
        {
            if (menuPrefab && menuSpawnPoint)
                attachedMenu = Instantiate(menuPrefab, menuSpawnPoint.position, Quaternion.identity, transform); menuAlreadySpawned = true;
        }

        if (!plateAlreadySpawned)
        {
            if (platePrefab && plateSpawnPoint)
                attachedPlate = Instantiate(platePrefab, plateSpawnPoint.position, Quaternion.identity, transform); plateAlreadySpawned = true;
        }

        if (attachedPlate != null)
        {
            var label = attachedPlate.GetComponentInChildren<LabelDisplay>();
            if (label != null) label.SetLabelFromId(customerId);
        }

        if (state == NPCState.Arrived && !patienceAlreadyStarted)
        {
            GetComponent<NPCPatience>()?.StartPatience();
            patienceAlreadyStarted = true;
        }
    }

    public void ForceEscape()
    {
        if (state == NPCState.Escaping) return;

        Debug.Log("ForceEscape called on NPC " + customerId);
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
            Destroy(attachedMenu);
            attachedMenu = null;
        }
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
            plateRoot.localPosition = plateReceiveAnchor ? Vector3.zero : plateSpawnPoint.localPosition;

            npcCollider.enabled = false; // Disable their collider when also taken food to avoid plate collider pushing the customer to run like hell
            Rigidbody2D plateRb = plateRoot.GetComponent<Rigidbody2D>();
            if (plateRb) plateRb.bodyType = RigidbodyType2D.Kinematic;
            AudioManager.Instance.PlaySound("yes", 1f, transform.position);
            state = NPCState.Leaving;
            currentWaypointIndex = 0;

            scoreManager.AddScore(score);

            if (attachedMenu != null)
            {
                Destroy(attachedMenu);
                attachedMenu = null;
            }

            return true;
        }
        return false;
    }

    public void FrustratedLeaving()
    {
        if (state == NPCState.Frustrated) return;

        Debug.Log("NPC " + customerId + " is now frustrated and leaving.");

        state = NPCState.Frustrated;
        currentWaypointIndex = 0;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        if (attachedMenu != null)
        {
            Destroy(attachedMenu);
            attachedMenu = null;
        }

        // Disable collider to avoid physical push during frustration exit
        npcCollider.enabled = false;

        //AudioManager.Instance.PlaySound("frustrated", 1f, transform.position);
    }

    private void MoveAlongPath(Vector3[] path)
    {
        if (path == null || path.Length == 0) return;

        if (currentWaypointIndex >= path.Length)
        {
            if (state == NPCState.Leaving || state == NPCState.Escaping || state == NPCState.Frustrated) // This is where when the enemy will remove itself when arrive at this point
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
            Debug.DrawRay(rb.position, direction * detectionRange, Color.red);
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
            if (attachedMenu != null)
            {
                Destroy(attachedMenu);
                attachedMenu = null;
            }
            ForceEscape();
            npcCollider.enabled = false; // Disable their collider when escaping
            AudioManager.Instance.PlaySound("scream", 1f, transform.position);
        }

        if (state == NPCState.Arrived)
        {
            returningToArrivedPoint = true;
            return;
        }
    }
}