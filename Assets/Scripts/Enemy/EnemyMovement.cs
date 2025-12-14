using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Enemy movement Fixed:
/// - operator precedence in OnCollisionEnter2D
/// - combinedMask uses .value
/// - null checks for agent/rb2d/TargetPos/cmty
/// - store lastDetectedTarget so we can chase actual GameObject if desired
/// - removed reassigning hits inside DetectTargets loop
/// - better coroutine nulling
/// </summary>
public class EnemyMovement : MonoBehaviour
{
    public bool FlyingEnemy;

    public GameObject Nulled;
    private CannotMoveThisWay cmty;
    public float speed = 3;
    public bool MoveNext = true;
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
    public bool pauseAttackAni;
    public bool TargetFound;

    public NavMeshAgent agent;
    Rigidbody2D rb2d;

    private Coroutine attackCoroutine;
    private Coroutine wanderCoroutine;
    private Coroutine waitCoroutine;
    private Coroutine cannotMoveCoroutine;

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
    private int combinedMask;

    public GameObject enemyMovePoint;

    // store the last detected GameObject (player/food) for chase logic
    private GameObject lastDetectedTarget;

    private EnemySpawner enemySpawner;
    private JiggleForSprite jiggleForSprite;

    private void OnEnable()
    {
        MoveNext = false;

        // Reset any lost state
        currentState = EnemyState.Wandering;
        StartCoroutine(ResumeDetectionAfterEnable());
    }

    private void OnDisable()
    {
        if (enemySpawner != null && enemySpawner.currentSpawnedEnemy > 0)
        {
            enemySpawner.currentSpawnedEnemy--;
        }
    }

    private IEnumerator ResumeDetectionAfterEnable()
    {
        // Give a short delay so NavMeshAgent fully initializes
        yield return new WaitForSeconds(0.1f);

        MoveNext = true;

        if (aimForFood)
        {
            DetectTargets(); // Try find food immediately
        }
        else
        {
            EnemyWonderAround(); // Resume normal wandering
        }
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb2d = GetComponent<Rigidbody2D>();

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning("GameObject PathFinder is missing");
            return;
        }

        if (FlyingEnemy)
        {
            if (agent != null) agent.enabled = false;
        }
        else
        {
            if (agent != null)
            {
                agent.updateRotation = false;
                agent.updateUpAxis = false;
            }
        }

        // instantiate TargetPos safely
        if (TargetPos == null)
        {
            Debug.LogError($"{name}: TargetPos prefab is not assigned.");
        }
        else
        {
            TargetPos = Instantiate(TargetPos);
            TargetPos.name = name + " TargetPos";
            TargetPos.transform.position = transform.position;
            TargetPos.transform.parent = this.gameObject.transform.parent;
        }

        if (Nulled != null)
        {
            ProjectileStorage = Instantiate(Nulled);
            ProjectileStorage.name = name + " Projectile";
            ProjectileStorage.transform.parent = this.gameObject.transform.parent;
        }

        player = GameObject.FindGameObjectsWithTag("Player");

        StartCoroutine(CreateEnemyField());

        cmty = GetComponentInChildren<CannotMoveThisWay>();
        if (cmty == null) Debug.LogWarning($"{name}: CannotMoveThisWay child not found.");

        EnemyGrid.RemoveAll(item => item == null);
        if (EnemyGrid.Count != 0)
            TargetedGrid = EnemyGrid[Random.Range(0, EnemyGrid.Count)].transform;

        // combine masks explicitly
        combinedMask = playerLayers.value | foodLayers.value | obstacleLayers.value;
        //Debug.Log("Combined mask: " + combinedMask);

        StartCoroutine(ChangeTargetedGrid(Random.Range(1f, 3f)));

        if (enemyMovePoint == null)
        {
            var comp = GetComponentInChildren<GetMeFormOtherCode>();
            if (comp != null) enemyMovePoint = comp.gameObject;
            else Debug.LogWarning($"{name}: GetMeFormOtherCode child not found.");
        }

        //enemySpawner = FindAnyObjectByType<EnemySpawner>().GetComponent<EnemySpawner>();
        enemySpawner = FindAnyObjectByType<EnemySpawner>();
        if (enemySpawner != null)
        {
            enemySpawner = enemySpawner.GetComponent<EnemySpawner>();
        }

        jiggleForSprite = GetComponentInChildren<JiggleForSprite>();
    }

    private void Update()
    {
        if (!FlyingEnemy && rb2d != null) 
        { 
            rb2d.velocity = Vector2.zero; 
            rb2d.angularVelocity = 0f; 
        }

        //if (transform.localScale.x < 0) //make sure the gameObject Scale.x won't get minus
        //{
        //    transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        //}
    }

    private void FixedUpdate()
    {
        DetectTargets();
        EnemyFoundTarget(player);   
    }

    private IEnumerator CreateEnemyField()
    {
        if (Nulled == null)
        {
            Debug.LogWarning($"{name}: Nulled prefab required for grid center creation.");
            yield break;
        }

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

                if (GPM != null)
                {
                    GPM.enemyMovement = this;
                    GPM.flying = FlyingEnemy;
                }

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

        MidOfSpawnedGrid.transform.position = this.gameObject.transform.position;
        MidOfSpawnedGrid.transform.localScale = Vector3.one * gapBetweenGrid;
        MidOfSpawnedGrid.transform.parent = this.gameObject.transform.parent;
    }

    private float GetSightRange()
    {
        return sightSize;
    }

    private void enemyMovement(Transform Target)
    {
        if (cmty != null && cmty.canMoveThisWay == false)
        {
            if (!FlyingEnemy && agent != null)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }

            if (cannotMoveCoroutine == null)
            {
                cannotMoveCoroutine = StartCoroutine(CannotMoveTimeout());
            }

            if (!isAttacking)
            {
                if (attackCoroutine != null) { StopCoroutine(attackCoroutine); attackCoroutine = null; }
                attackCoroutine = StartCoroutine(Attack(DurationBeforeAttack, RecoveryFrame));
            }

            isMoving = false;
            return;
        }
        else
        {
            if (cannotMoveCoroutine != null)
            {
                StopCoroutine(cannotMoveCoroutine);
                cannotMoveCoroutine = null;
            }
        }

        if (!FlyingEnemy && agent != null && agent.isStopped)
            agent.isStopped = false;

        if (Target == null) return;

        if (Vector3.Distance(transform.position, Target.transform.position) > 0.1f)
            enemyRotation(Target);

        if (!FlyingEnemy && agent != null)
        {
            agent.SetDestination(Target.position);
            isMoving = agent.velocity.magnitude > 0.05f;
        }
        else if (rb2d != null)
        {
            Vector2 newPos = Vector2.MoveTowards(rb2d.position, Target.position, speed * Time.deltaTime);
            isMoving = Vector2.Distance(rb2d.position, newPos) > 0.01f;
            rb2d.MovePosition(newPos);
            rb2d.velocity = Vector2.zero;
        }
    }

    private IEnumerator CannotMoveTimeout() //don't trigger this when enemy is attacking
    {
        float delay = 3f;
        yield return new WaitForSeconds(delay);

        if (cmty != null && cmty.canMoveThisWay == false)
        {
            Debug.Log("Stuck too long, switching to new wander point...");
            currentState = EnemyState.Wandering;
            StartCoroutine(ChangeTargetedGrid(0));
        }

        cannotMoveCoroutine = null;
    }

    private void enemyRotation(Transform Target)
    {
        if (enemyMovePoint == null || Target == null) return;

        Vector2 direction = Target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, Mathf.Abs(direction.x)) * Mathf.Rad2Deg;

        if (direction.x < 0)
            angle = 180f - angle;

        Quaternion Rotation = Quaternion.Euler(0, 0, angle);
        enemyMovePoint.transform.rotation = Rotation;
    }

    private void EnemyWonderAround()
    {
        if (TargetedGrid == null || Vector3.Distance(transform.position, TargetedGrid.position) < 1f)
        {
            if (MoveNext)
            {
                wanderCoroutine = StartCoroutine(ChangeTargetedGrid(Random.Range(1f, 3f)));
            }
        }

        if (TargetedGrid != null) enemyMovement(TargetedGrid);
    }

    private void EnemyFoundTarget(GameObject[] Targets)
    {
        if (TargetFound && currentState != EnemyState.Chasing && currentState != EnemyState.WaitingToReturn && !justReturnedFromWait)
        {
            if (lastDetectedTarget != null)
                SwitchToChaseMode(lastDetectedTarget);
            else if (TargetPos != null)
                SwitchToChaseMode(TargetPos);

            return;
        }

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
                        if (agent != null && !agent.pathPending && agent.remainingDistance <= 0.1f)
                            reachedDestination = true;
                    }
                    else
                    {
                        if (TargetedGrid != null && Vector3.Distance(transform.position, TargetedGrid.position) <= 0.1f)
                            reachedDestination = true;
                    }

                    if (reachedDestination)
                    {
                        currentState = EnemyState.WaitingToReturn;
                        if (waitCoroutine != null) StopCoroutine(waitCoroutine);
                        waitCoroutine = StartCoroutine(WaitAfterLosingPlayer(Random.Range(1f, 3f)));
                        isMovingToLastSeenPos = false;
                    }
                }
                break;

            case EnemyState.WaitingToReturn:
                // idle
                break;
        }
    }

    private void SwitchToChaseMode(GameObject playerTarget)
    {
        if (playerTarget == null) return;

        if (wanderCoroutine != null) { StopCoroutine(wanderCoroutine); wanderCoroutine = null; }
        if (waitCoroutine != null) { StopCoroutine(waitCoroutine); waitCoroutine = null; }

        currentState = EnemyState.Chasing;

        if (TargetPos != null)
            TargetPos.transform.position = playerTarget.transform.position;

        TargetedGrid = TargetPos != null ? TargetPos.transform : playerTarget.transform;

        isMovingToLastSeenPos = true;

        enemyRotation(playerTarget.transform);
        enemyMovement(TargetedGrid);
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

        waitCoroutine = null;
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
            if (TargetPos != null) TargetPos.transform.position = TargetedGrid.transform.position;
        }

        MoveNext = true;
        wanderCoroutine = null;
    }

    private IEnumerator Attack(float dba, float recoverFrame)
    {
        isAttacking = true;
        pauseAttackAni = true;

        if (jiggleForSprite != null) jiggleForSprite.StartJiggle();

        yield return new WaitForSeconds(dba);

        if (Hitbox != null) Hitbox.SetActive(true);

        yield return new WaitForSeconds(0.2f);

        if (Hitbox != null) Hitbox.SetActive(false);
        if (jiggleForSprite != null) jiggleForSprite.StopJiggle();


        yield return new WaitForSeconds(recoverFrame);

        pauseAttackAni = false;
        isAttacking = false;
        attackCoroutine = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightSize);
    }

    private bool CanReachTarget(Vector3 targetPosition)
    {
        if (FlyingEnemy) return true;
        if (agent == null) return false;

        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(targetPosition, path);
        return path.status == NavMeshPathStatus.PathComplete;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // FIXED: ensure grouping so (Wall || Obstacle) && state == Chasing
        if ((collision.gameObject.layer == LayerMask.NameToLayer("Wall") ||
             collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
            && currentState == EnemyState.Chasing)
        {
            Debug.Log("Hit wall, pausing chase.");

            currentState = EnemyState.WaitingToReturn;
            if (waitCoroutine != null) StopCoroutine(waitCoroutine);
            waitCoroutine = StartCoroutine(WaitAfterLosingPlayer(Random.Range(1f, 3f)));
            isMovingToLastSeenPos = false;
        }
    }

    private void DetectTargets()
    {
        TargetFound = false;
        lastDetectedTarget = null;

        // Pick correct mask
        LayerMask mask = aimForFood ? foodLayers : playerLayers;

        // Find all colliders in range
        hits = Physics2D.OverlapCircleAll(transform.position, sightSize, mask);

        foreach (var hit in hits)
        {
            if (hit == null) continue;

            // For aimForFood enemies: skip anything without HealthManager
            if (aimForFood)
            {
                if (hit.GetComponent<HealthManager>() == null && 
                    hit.GetComponentInChildren<HealthManager>() == null ||
                    hit.GetComponent<ItemSystem>() == null ||
                    hit.GetComponent<ItemSystem>().isBurned
                    )
                    continue;
            }

            // For non-aimForFood enemies: skip if not Player
            else
            {
                if (!hit.CompareTag("Player"))
                    continue;
            }

            // Visibility check (raycast)
            Vector2 dirToTarget = hit.transform.position - transform.position;
            float sightRange = GetSightRange();
            RaycastHit2D ray = Physics2D.Raycast(transform.position, dirToTarget.normalized, sightRange, combinedMask);
            Debug.DrawRay(transform.position, dirToTarget.normalized * sightRange, Color.red, 0.1f);

            // Only confirm if the thing hit by ray IS the thing we're targeting
            if (ray.collider != null && ray.collider.gameObject == hit.gameObject)
            {
                TargetFound = true;
                lastDetectedTarget = hit.gameObject;

                if (TargetPos != null)
                    TargetPos.transform.position = hit.transform.position;

                break;
            }
        }
    }
}