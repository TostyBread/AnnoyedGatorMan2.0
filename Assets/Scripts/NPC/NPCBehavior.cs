using UnityEngine;

public class NPCBehavior : MonoBehaviour
{
    public float moveSpeed = 2f;
    public Transform menuSpawnPoint;
    public Transform plateSpawnPoint;

    private Vector3[] waypoints;
    private int currentWaypointIndex = 0;
    private bool hasArrived = false;

    private GameObject menuPrefab;
    private GameObject platePrefab;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (!hasArrived && waypoints != null && waypoints.Length > 0)
        {
            MoveToWaypoint();
        }
    }

    public void SetWaypoints(Vector3[] path)
    {
        waypoints = path;
        currentWaypointIndex = 0;
        hasArrived = false;
    }

    public void SetMenuAndPlatePrefabs(GameObject menu, GameObject plate)
    {
        menuPrefab = menu;
        platePrefab = plate;
    }

    private void MoveToWaypoint()
    {
        Vector3 targetPosition = waypoints[currentWaypointIndex];
        Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);

        if (Vector2.Distance(rb.position, targetPosition) < 0.1f)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Length)
            {
                hasArrived = true;
                SpawnMenuAndPlate();
            }
        }
    }

    private void SpawnMenuAndPlate()
    {
        if (menuPrefab && menuSpawnPoint)
            Instantiate(menuPrefab, menuSpawnPoint.position, Quaternion.identity, transform);

        if (platePrefab && plateSpawnPoint)
            Instantiate(platePrefab, plateSpawnPoint.position, Quaternion.identity, transform);
    }
}