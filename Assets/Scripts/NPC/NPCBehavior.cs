using UnityEngine;

public class NPCBehavior : MonoBehaviour
{
    public float moveSpeed = 2f;
    public Transform menuSpawnPoint;
    public Transform plateSpawnPoint;

    private Vector3 targetPosition;
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
        if (!hasArrived)
        {
            MoveToTarget();
        }
    }

    public void SetTarget(Vector3 target)
    {
        targetPosition = target;
    }

    public void SetMenuAndPlatePrefabs(GameObject menu, GameObject plate)
    {
        menuPrefab = menu;
        platePrefab = plate;
    }

    private void MoveToTarget()
    {
        Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);

        if (Vector2.Distance(rb.position, targetPosition) < 0.1f)
        {
            hasArrived = true;
            SpawnMenuAndPlate();
        }
    }

    private void SpawnMenuAndPlate()
    {
        if (menuPrefab && menuSpawnPoint)
            Instantiate(menuPrefab, menuSpawnPoint.position, Quaternion.identity, transform);

        if (platePrefab && plateSpawnPoint)
            Instantiate(platePrefab, plateSpawnPoint.position, Quaternion.identity, transform);
    }

    //private void OnTriggerEnter2D(Collider2D other)
    //{
    //    PlateSystem plateSystem = other.GetComponent<PlateSystem>();

    //    if (plateSystem.foodComplete == true)
    //    {
    //        Debug.Log("Yay, my food is here!");
    //    }
    //    else Debug.Log("My food isn't complete!");
    //}
}