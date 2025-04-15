using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyMovement : MonoBehaviour
{
    public GameObject Nulled;
    private CannotMoveThisWay cmty;
    public float speed = 3;
    private bool MoveNext = true;
    private Transform TargetedGrid;
    public float gapBetweenGrid;

    private GameObject player;

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

    private bool isChasingPlayer = false;
    private bool isWaitingAfterLost = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        CreateEnemyField();
        CreateEnemySight();

        cmty = GetComponentInChildren<CannotMoveThisWay>();
        TargetedGrid = EnemyGrid[Random.Range(0, EnemyGrid.Count)].transform;
        StartCoroutine(ChangeTargetedGrid(Random.Range(1f, 3f)));
    }

    private void Update()
    {
        MOSP.transform.position = transform.position;

        EnemyFindTarget(player);
    }

    private void CreateEnemyField()
    {
        MidOfSpawnedGrid = Instantiate(Nulled);
        MidOfSpawnedGrid.name = name + "'s move grid";

        for (int x = 0; x < MoveGridSize; x++)
        {
            for (int y = 0; y < MoveGridSize; y++)
            {
                Vector3 pos = new Vector3(x, y, 0);
                GameObject SpawnedGrid = Instantiate(SeeGrid, pos, transform.rotation);
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

        MidOfSpawnedGrid.transform.position = this.gameObject.transform.position;
        MidOfSpawnedGrid.transform.localScale = Vector3.one * gapBetweenGrid;
    }

    private void CreateEnemySight()
    {
        MOSP = Instantiate(Nulled);
        MOSP.name = name + "'s sight grid";

        for (int x = 0; x < sightSize; x++)
        {
            for (int y = 0; y < sightSize; y++)
            {
                Vector3 pos = new Vector3(x, y, 0);
                GameObject SpawnedGrid = Instantiate(SightGrid, pos, transform.rotation);
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

        MOSP.transform.position = this.gameObject.transform.position;
        MOSP.transform.localScale = Vector3.one * gapBetweenGrid;
    }

    private void enemyMovement(Transform Target)
    {
        if (cmty.canMoveThisWay == false)
        {
            //return;
        }

        transform.position = Vector2.MoveTowards(transform.position,Target.transform.position,speed * Time.deltaTime);

        if (transform.position != Target.position)
        {
            enemyRotation(Target);
        }
    }

    private void enemyRotation(Transform Target)
    {
        Vector2 direction = Target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void EnemyWonderAround()
    {
        // If we’ve reached the destination, choose a new one after a delay
        if (TargetedGrid == null || Vector3.Distance(transform.position, TargetedGrid.position) < 0.1f)
        {
            if (MoveNext)
            {
                StartCoroutine(ChangeTargetedGrid(Random.Range(1f, 3f)));
            }
        }

        // Keep moving toward the current target if it exists
        if (TargetedGrid != null)
        {
            enemyMovement(TargetedGrid);
        }
    }

    private void EnemyFindTarget(GameObject Target)
    {
        bool Detected = false;

        foreach (var sight in EnemySight)
        {
            if (Vector3.Distance(Target.transform.position, sight.transform.position) < 1f * gapBetweenGrid)
            {
                if (Target != null)
                {
                    Detected = true;

                    if (!isChasingPlayer)
                    {
                        Debug.Log("target detected = " + Target);
                        StopAllCoroutines(); //stop waiting and random change
                        MoveNext = false;

                        isChasingPlayer = true;
                    }

                    TargetedGrid = Target.transform;
                    enemyMovement(TargetedGrid);
                    break;
                }
            }
        }

        if (!Detected)
        {
            if (isChasingPlayer)
            {
                if (!isWaitingAfterLost)
                {
                    StartCoroutine(WaitAfterLosingPlayer(Random.Range(1f,3f)));
                    isChasingPlayer = false;
                }

            }

            if (!isWaitingAfterLost)
            {
                EnemyWonderAround();
            }
        }
    }

    private IEnumerator WaitAfterLosingPlayer(float delay)
    { 
        isWaitingAfterLost = true;
        Debug.Log("player lost, wait " + delay + " seconds");

        yield return new WaitForSeconds(delay);

        isWaitingAfterLost = false;
        StartCoroutine(ChangeTargetedGrid(0));
    }

    private IEnumerator ChangeTargetedGrid(float delay)
    {
        MoveNext = false;
        yield return new WaitForSeconds(delay);
        Debug.Log("Change targeted grid after " + delay + " seconds");

        TargetedGrid = EnemyGrid[Random.Range(0, EnemyGrid.Count)].transform;
        Debug.Log("Target Grid Changed");

        MoveNext = true;
    }
}
