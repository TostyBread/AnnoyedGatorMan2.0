using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    public bool FlyingEnemy;

    public GameObject Nulled;
    private CannotMoveThisWay cmty;
    public float speed = 3;
    private bool MoveNext = true;
    public float gapBetweenGrid;

    public GameObject TargetPos;

    private GameObject ProjectileStorage;
    public GameObject Projectile;

    public bool CanShoot;
    private float time;
    public float ShootIntervel;

    [SerializeField] private GameObject Hitbox;
    [SerializeField] private float DurationBeforeAttack;
    [SerializeField] private float RecoveryFrame;

    [Header("Just for debug, do not touch")]
    [SerializeField] private Transform TargetedGrid;
    private GameObject[] player;

    public bool isMoving;
    public bool isAttacking;
    public bool TargetFound;

    public NavMeshAgent agent;
    Rigidbody2D rb2d;

    private Coroutine attackCoroutine;
    private Coroutine wanderCoroutine;
    private Coroutine waitCoroutine;

    public enum EnemyState
    {
        Wandering,
        Chasing,
        WaitingToReturn
    }
    public EnemyState currentState = EnemyState.Wandering;
    private bool isMovingToLastSeenPos = false;

    [Header("Enemy Move Grid")]
    public GameObject SeeGrid;
    public int MoveGridSize = 5;
    [HideInInspector] public GameObject MidOfSpawnedGrid;
    public List<GameObject> EnemyGrid = new List<GameObject>();
    private bool onceMoveGrid;

    [Header("Enemy Sight Grid")]
    public GameObject SightGrid;
    public int sightSize = 3;
    [HideInInspector] public GameObject MOSP;
    public List<GameObject> EnemySight = new List<GameObject>();
    private bool onceSightGrid;

    Collider2D[] hits;
    public bool aimForFood;
    private bool justReturnedFromWait = false;

    [SerializeField] private LayerMask playerLayers;
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField] private LayerMask foodLayers;
    int combinedMask;

    public GameObject enemyMovePoint;


    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb2d = GetComponent<Rigidbody2D>();

        if (FlyingEnemy)
        {
            agent.enabled = false; // Disable NavMeshAgent for flying enemies

            //rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        else
        {
            agent.updateRotation = false; // Only necessary for grounded enemies using NavMesh
            agent.updateUpAxis = false;   // Ensures 2D rotation on Z axis
        }


        //instantiate empty gameobject to record TargetPos
        TargetPos = Instantiate(TargetPos);
        TargetPos.name = name + " TargetPos";
        TargetPos.transform.position = transform.position;
        TargetPos.transform.parent = this.gameObject.transform.parent;


        //instantiate empty gameobject to store current enemy shooted projectile
        ProjectileStorage = Instantiate(Nulled);
        ProjectileStorage.name = name + " Projectile";
        ProjectileStorage.transform.parent = this.gameObject.transform.parent;

        player = GameObject.FindGameObjectsWithTag("Player");

        StartCoroutine(CreateEnemyField());
        //StartCoroutine(CreateEnemySight());

        //CreateEnemyField();
        //CreateEnemySight();

        cmty = GetComponentInChildren<CannotMoveThisWay>();

        EnemyGrid.RemoveAll(item => item == null);
        if (EnemyGrid.Count != 0)
            TargetedGrid = EnemyGrid[Random.Range(0, EnemyGrid.Count)].transform;

        combinedMask = playerLayers | foodLayers | obstacleLayers;
        Debug.Log("Combined mask: " + combinedMask);
    

        StartCoroutine(ChangeTargetedGrid(Random.Range(1f, 3f)));

        if (enemyMovePoint == null)
        enemyMovePoint = gameObject.GetComponentInChildren<GetMeFormOtherCode>().gameObject;
    }

    private void FixedUpdate()
    {
        DetectTargets();
        EnemyFoundTarget(player);
    }

    private IEnumerator CreateEnemyField()
    {
        MidOfSpawnedGrid = Instantiate(Nulled);
        MidOfSpawnedGrid.name = name + " move grid";
        MidOfSpawnedGrid.layer = LayerMask.NameToLayer("Ignore Me");

        yield return new WaitForSeconds(0.5f);

        for (int x = 0; x < MoveGridSize; x++)
        {
            for (int y = 0; y < MoveGridSize; y++)
            {
                Vector3 pos = new Vector3(x, y, 0);
                GameObject SpawnedGrid = Instantiate(SeeGrid, pos, transform.rotation);
                GridPointManager GPM = SpawnedGrid.GetComponent<GridPointManager>();

                GPM.enemyMovement = gameObject.GetComponent<EnemyMovement>();
                GPM.flying = FlyingEnemy;

                //SpawnedGrid.transform.parent = MidOfSpawnedGrid.transform;

                if (x >= MoveGridSize / 2 && y >= MoveGridSize / 2 && !onceMoveGrid)
                {
                    MidOfSpawnedGrid.transform.position = SpawnedGrid.transform.position;
                    onceMoveGrid = true;
                }

                EnemyGrid.Add(SpawnedGrid);
            }
        }

        foreach (var SpawnedGrid in EnemyGrid)
        {
            SpawnedGrid.transform.parent = MidOfSpawnedGrid.transform;
        }

        //set the points into MidOfSpawnedGrid gameObject & adjust the scale of MOSP
        MidOfSpawnedGrid.transform.position = this.gameObject.transform.position;
        MidOfSpawnedGrid.transform.localScale = Vector3.one * gapBetweenGrid;
        //result is the gap between points will be wider or narrower
        
        MidOfSpawnedGrid.transform.parent = this.gameObject.transform.parent;
    }

    private IEnumerator CreateEnemySight()
    {
        MOSP = Instantiate(Nulled);
        MOSP.name = name + " sight grid";
        MOSP.layer = LayerMask.NameToLayer("Ignore Me");

        yield return new WaitForSeconds(0.5f);

        for (int x = 0; x < sightSize; x++)
        {
            for (int y = 0; y < sightSize; y++)
            {
                Vector3 pos = new Vector3(x, y, 0);
                GameObject SpawnedGrid = Instantiate(SightGrid, pos, transform.rotation);
                GridPointManager GPM = SpawnedGrid.GetComponent<GridPointManager>();

                GPM.enemyMovement = gameObject.GetComponent<EnemyMovement>();
                GPM.flying = FlyingEnemy;

                //SpawnedGrid.transform.parent = MidOfSpawnedGrid.transform;

                if (x >= sightSize / 2 && y >= sightSize / 2 && !onceSightGrid)
                {
                    MOSP.transform.position = SpawnedGrid.transform.position;
                    onceSightGrid = true;
                }

                EnemySight.Add(SpawnedGrid);
            }
        }

        foreach (var SpawnedGrid in EnemySight)
        {
            SpawnedGrid.transform.parent = MOSP.transform;
        }

        //set the points into MOSP gameObject & adjust the scale of MOSP
        MOSP.transform.position = this.gameObject.transform.position;
        MOSP.transform.localScale = Vector3.one * gapBetweenGrid;
        //result is the gap between points will be wider or narrower

        MOSP.transform.parent = this.gameObject.transform.parent;
    }

    private float GetSightRange()
    {
        return sightSize;

        //float maxDistance = 0f;

        //foreach (GameObject sightPoint in EnemySight)
        //{
        //    float distance = Vector2.Distance(transform.position, sightPoint.transform.position);
        //    if (distance > maxDistance)
        //    {
        //        maxDistance = distance;
        //    }
        //}

        //return maxDistance;
    }

    private void enemyMovement(Transform Target)
    {
        if (cmty.canMoveThisWay == false)
        {
            if (!FlyingEnemy)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }

            // Handle attack logic
            if (!isAttacking)
            {
                attackCoroutine = StartCoroutine(Attack(DurationBeforeAttack, RecoveryFrame));
            }

            isMoving = false; // Not moving due to blocked path
            return;
        }

        if (!FlyingEnemy && agent.isStopped)
            agent.isStopped = false;

        if (transform.position != Target.transform.position)
            enemyRotation(Target);

        // Movement logic + isMoving logic
        if (!FlyingEnemy)
        {
            agent.SetDestination(Target.position);
            isMoving = agent.velocity.magnitude > 0.05f; // small threshold to prevent false positives
        }
        else
        {
            Vector2 newPos = Vector2.MoveTowards(rb2d.position, Target.position, speed * Time.deltaTime);
            isMoving = Vector2.Distance(rb2d.position, newPos) > 0.01f;
            rb2d.MovePosition(newPos);
        }

        rb2d.velocity = Vector2.zero; // Remove any physics force drag
    }

    private void enemyRotation(Transform Target)
    {
        Vector2 direction = Target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion Rotation = Quaternion.Euler(0, 0, angle);
        
        enemyMovePoint.transform.rotation = Rotation;
        //transform.rotation = Quaternion.Lerp(transform.rotation, Rotation, Time.deltaTime * 5f);
    }

    private void EnemyWonderAround()
    {
        // If we’ve reached the destination, choose a new one after a delay
        if (TargetedGrid == null || Vector3.Distance(transform.position, TargetedGrid.position) < 1f)
        {
            if (MoveNext)
            {
                wanderCoroutine = StartCoroutine(ChangeTargetedGrid(Random.Range(1f, 3f)));
            }
        }

        // Keep moving toward the current target if it exists
        if (TargetedGrid != null)
        {
            enemyMovement(TargetedGrid);
        }
    }

    private void EnemyFoundTarget(GameObject[] Targets)
    {
        if (TargetFound && currentState != EnemyState.Chasing && currentState != EnemyState.WaitingToReturn && !justReturnedFromWait)
        {
            SwitchToChaseMode(TargetPos);
            return;
        }

        // If not detected, go with normal behavior per state
        switch (currentState)
        {
            case EnemyState.Wandering:
                EnemyWonderAround();
                break;

            case EnemyState.Chasing:
                if (isMovingToLastSeenPos)
                {
                    enemyMovement(TargetedGrid);

                    bool reachedDestination = false;

                    if (!FlyingEnemy)
                    {
                        // Use NavMeshAgent's pathfinding state
                        if (!agent.pathPending && agent.remainingDistance <= 0.1f)
                        {
                            reachedDestination = true;
                        }
                    }
                    else
                    {
                        // Use transform distance for flying enemies
                        if (Vector3.Distance(transform.position, TargetedGrid.position) <= 0.1f)
                        {
                            reachedDestination = true;
                        }
                    }

                    if (reachedDestination)
                    {
                        //Debug.Log("Reached last known player position. Begin wait...");
                        currentState = EnemyState.WaitingToReturn;
                        waitCoroutine = StartCoroutine(WaitAfterLosingPlayer(Random.Range(1f, 3f)));
                        isMovingToLastSeenPos = false;
                    }
                }
                break;


            case EnemyState.WaitingToReturn:
                // Do nothing while waiting, unless interrupted by detection above
                break;
        }
    }

    private void SwitchToChaseMode(GameObject playerTarget)
    {
        // Cancel wandering/waiting
        if (wanderCoroutine != null) StopCoroutine(wanderCoroutine);
        if (waitCoroutine != null) StopCoroutine(waitCoroutine);

        currentState = EnemyState.Chasing;

        TargetPos.transform.position = playerTarget.transform.position;
        TargetedGrid = TargetPos.transform;

        isMovingToLastSeenPos = true;

        enemyRotation(playerTarget.transform);
        enemyMovement(TargetedGrid);
    }

    private GameObject GetClosestTarget(GameObject[] targets)
    {
        float minDistance = Mathf.Infinity;
        GameObject closest = null;

        foreach (var t in targets)
        {
            float dist = Vector3.Distance(transform.position, t.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = t;
            }
        }

        return closest;
    }

    private IEnumerator WaitAfterLosingPlayer(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (currentState == EnemyState.WaitingToReturn)
        {
            currentState = EnemyState.Wandering;

            justReturnedFromWait = true;
            StartCoroutine(GracePeriodAfterReturn(2f));

            StartCoroutine(ChangeTargetedGrid(0));
        }
    }

    private IEnumerator GracePeriodAfterReturn(float duration)
    {
        yield return new WaitForSeconds(duration);
        justReturnedFromWait = false;
    }

    private IEnumerator ChangeTargetedGrid(float delay)
    {
        MoveNext = false;
        yield return new WaitForSeconds(delay);

        if (EnemyGrid.Count > 0)
        {
            TargetedGrid = EnemyGrid[Random.Range(0, EnemyGrid.Count)].transform;
            TargetPos.transform.position = TargetedGrid.transform.position;
            //Debug.Log("Target Grid Changed to " + TargetedGrid.name);
        }

        MoveNext = true;
    }

    private IEnumerator Attack(float dba,float recoverFrame)
    {
        isAttacking = true;

        //Debug.Log("ready to attack " + dba);
        yield return new WaitForSeconds(dba);

        //Debug.Log("attack");
        Hitbox.SetActive(true);

        yield return new WaitForSeconds(recoverFrame);
        isAttacking = false;
    }

    // draw the target area
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightSize);
    }

    private bool CanReachTarget(Vector3 targetPosition)
    {
        if (FlyingEnemy) return true; // Flying enemies can always "fly" to targets

        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(targetPosition, path);

        // Path is complete only if it is valid and not partial or invalid
        return path.status == NavMeshPathStatus.PathComplete;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall") 
            && currentState == EnemyState.Chasing)
        {
            Debug.Log("Hit wall, pausing chase.");

            // Stop chasing and wait before returning
            currentState = EnemyState.WaitingToReturn;
            waitCoroutine = StartCoroutine(WaitAfterLosingPlayer(Random.Range(1f, 3f)));
            isMovingToLastSeenPos = false;
        }
    }
    private void DetectTargets()
    {
        TargetFound = false;

        LayerMask mask = aimForFood ? foodLayers : playerLayers;
        hits = Physics2D.OverlapCircleAll(transform.position, sightSize, mask);

        foreach (var hit in hits)
        {
            if (hit == null) continue;
            if (hit.GetComponent<HealthManager>() == null) continue;

            Vector2 dirToTarget = hit.transform.position - transform.position;
            float sightRange = GetSightRange();
            RaycastHit2D ray = Physics2D.Raycast(
                transform.position,
                dirToTarget.normalized,
                sightRange,
                combinedMask
            );
            Debug.DrawRay(transform.position, dirToTarget.normalized * sightRange, Color.red, 0.1f);

            //if (ray.collider != null)
            //{
            //    Debug.Log("Ray hit: " + ray.collider.name);
            //}
            //else
            //{
            //    Debug.Log("Ray missed");
            //}

            if (ray.collider != null && ray.collider.transform.root == hit.transform.root)
            {
                //if (!CanReachTarget(hit.transform.position)) continue;
                //Debug.Log("Comparing ray hit: " + ray.collider.name + " with target: " + hit.name);

                TargetFound = true;
                TargetPos.transform.position = hit.transform.position;
                break;
            }

            hits = Physics2D.OverlapCircleAll(transform.position, sightSize, mask);
            //Debug.Log("Targets in range: " + hits.Length);
        }
    }
}