using UnityEngine;

public class NPCBehavior : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float forceEscapeThreshold = 5f;
    public Transform menuSpawnPoint;
    public Transform plateSpawnPoint;
    public Transform plateReceiveAnchor;
    public int customerId { get; private set; }

    private Vector3[] waypoints;
    private Vector3[] exitWaypoints;
    private int currentWaypointIndex = 0;
    private bool hasArrived = false;
    private bool isLeaving = false;

    private GameObject menuPrefab;
    private GameObject platePrefab;
    private GameObject attachedPlate;
    private GameObject attachedMenu;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (!hasArrived && !isLeaving)
        {
            MoveAlongPath(waypoints);
        }
        else if (isLeaving && exitWaypoints != null && exitWaypoints.Length > 0)
        {
            MoveAlongPath(exitWaypoints);
        }
        else if (hasArrived && Vector2.Distance(rb.position, waypoints[^1]) > 0.1f)
        {
            rb.MovePosition(Vector2.MoveTowards(rb.position, waypoints[^1], moveSpeed * Time.fixedDeltaTime));
        }
    }

    public void SetWaypoints(Vector3[] path)
    {
        waypoints = path;
        currentWaypointIndex = 0;
        hasArrived = false;
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

    private void MoveAlongPath(Vector3[] path)
    {
        if (currentWaypointIndex >= path.Length)
        {
            if (isLeaving) Destroy(gameObject);
            return;
        }

        Vector3 targetPosition = path[currentWaypointIndex];
        Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);

        if (Vector2.Distance(rb.position, targetPosition) < 0.1f)
        {
            currentWaypointIndex++;
            if (!isLeaving && currentWaypointIndex >= path.Length)
                hasArrived = true;
        }
    }

    public void SetCustomerId(int id)
    {
        customerId = id;

        var label = GetComponentInChildren<LabelDisplay>();
        if (label != null)
        {
            label.SetLabelFromId(customerId);
        }
    }

    public void SpawnMenuAndPlate()
    {
        if (menuPrefab && menuSpawnPoint)
            attachedMenu = Instantiate(menuPrefab, menuSpawnPoint.position, Quaternion.identity, transform);

        if (platePrefab && plateSpawnPoint)
            attachedPlate = Instantiate(platePrefab, plateSpawnPoint.position, Quaternion.identity, transform);

        if (attachedPlate != null)
        {
            var label = attachedPlate.GetComponentInChildren<LabelDisplay>();
            if (label != null)
            {
                label.SetLabelFromId(customerId);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasArrived) return;

        PlateSystem plate = collision.GetComponentInChildren<PlateSystem>();
        if (plate != null)
        {
            TryAcceptPlate(plate);
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isLeaving) return;

        if (collision.relativeVelocity.magnitude > forceEscapeThreshold)
        {
            ForceEscape();
        }
    }

    public void ForceEscape()
    {
        if (isLeaving) return;

        isLeaving = true;
        hasArrived = true;
        currentWaypointIndex = 0;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.MovePosition(transform.position); // cancel ongoing movement

        if (attachedMenu != null)
        {
            Destroy(attachedMenu);
            attachedMenu = null;
        }

        Debug.Log($"NPC {customerId} forced to escape!");
    }

    public bool TryAcceptPlate(PlateSystem plate)
    {
        if (plate == null)
        {
            //Debug.LogWarning("Plate is null.");
            return false;
        }

        //Debug.Log("Plate found. Ready: " + plate.isReadyToServe);

        if (!plate.isReadyToServe) return false;

        var label = plate.GetComponentInChildren<LabelDisplay>();

        if (label != null && label.labelText == customerId.ToString())
        {
            //Debug.Log("Plate accepted by NPC " + customerId);

            Transform plateObj = plate.rootPlateObject != null ? plate.rootPlateObject : plate.transform;
            plateObj.SetParent(plateReceiveAnchor ?? transform);
            plateObj.localPosition = plateReceiveAnchor ? Vector3.zero : plateSpawnPoint.localPosition;

            Rigidbody2D rb = plateObj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.isKinematic = true;
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.simulated = false;
            }
            label.DisableLabel();
            Destroy(attachedMenu);
            attachedMenu = null;
            isLeaving = true;
            currentWaypointIndex = 0;
            return true;
        }

        //Debug.Log("Label mismatch or missing.");
        return false;
    }
}