using UnityEngine;

public class NPCBehavior : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float forceEscapeThreshold = 5f;
    public Transform menuSpawnPoint;
    public Transform plateSpawnPoint;
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
        if (waypoints == null || waypoints.Length == 0) return;

        if (!hasArrived)
        {
            MoveAlongPath(waypoints);
        }
        else if (isLeaving && exitWaypoints != null && exitWaypoints.Length > 0)
        {
            MoveAlongPath(exitWaypoints);
        }
        else if (hasArrived && Vector2.Distance(rb.position, waypoints[^1]) > 0.2f)
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
        Debug.Log($"NPC collided with {collision.name}");

        PlateSystem plate = collision.GetComponentInChildren<PlateSystem>();
        if (plate == null)
        {
            plate = collision.GetComponent<PlateSystem>();
        }

        if (plate != null)
        {
            Debug.Log("PlateSystem found on: " + plate.name);
            TryAcceptPlate(plate);
        }
        else
        {
            Debug.LogWarning("PlateSystem NOT found in collision.");
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasArrived || isLeaving) return;

        if (collision.relativeVelocity.magnitude > forceEscapeThreshold)
        {
            isLeaving = true;
            currentWaypointIndex = 0;

            if (attachedMenu != null)
            {
                Destroy(attachedMenu);
                attachedMenu = null;
            }
        }
    }

    public bool TryAcceptPlate(PlateSystem plate)
    {
        if (plate != null && plate.isReadyToServe)
        {
            var label = plate.GetComponentInChildren<LabelDisplay>();
            if (label != null && label.labelText == customerId.ToString())
            {
                plate.transform.SetParent(transform);
                plate.transform.localPosition = plateSpawnPoint.localPosition;
                isLeaving = true;
                currentWaypointIndex = 0;
                return true;
            }
        }
        return false;
    }
}