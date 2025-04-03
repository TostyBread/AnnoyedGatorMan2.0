using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;
using Random = UnityEngine.Random;

public class EnemyMovement : MonoBehaviour
{
    private bool once;
    private bool once2;
    public GameObject Nulled;
    private CannotMoveThisWay cmty;
    public float speed = 3;
    private bool MoveNext = true;
    public Transform TargetedGrid;

    private GameObject player;

    [Header("Enemy Move Grid")]
    public GameObject SeeGrid;
    public int MoveGridSize = 5;
    [HideInInspector] public GameObject MidOfSpawnedGrid;
    public List<GameObject> EnemyGrid = new List<GameObject>();

    [Header("Enemy Sight Grid")]
    public GameObject SightGrid;
    public int sightSize = 3;
    [HideInInspector] public GameObject MOSP;
    public List<GameObject> EnemySight = new List<GameObject>();

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        CreateEnemyField();
        once = false;
        CreateEnemySight();

        cmty = GetComponentInChildren<CannotMoveThisWay>();
        TargetedGrid = EnemyGrid[Random.Range(0, EnemyGrid.Count)].transform;
        StartCoroutine(ChangeTargetedGrid(Random.Range(1f, 3f)));
    }

    private void Update()
    {
       EnemyWonderAround();
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

                if (x >= MoveGridSize / 2 && y >= MoveGridSize / 2 && once == false)
                {
                    MidOfSpawnedGrid.transform.position = SpawnedGrid.transform.position;
                    once = true;
                }

                EnemyGrid.Add(SpawnedGrid);
            }
        }

        foreach (var SpawnedGrid in EnemyGrid)
        {
            SpawnedGrid.transform.parent = MidOfSpawnedGrid.transform;
        }

        MidOfSpawnedGrid.transform.position = this.gameObject.transform.position;
        MidOfSpawnedGrid.transform.localScale = Vector3.one * 1.5f;
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

                if (x >= sightSize / 2 && y >= sightSize / 2 && once == false)
                {
                    MOSP.transform.position = SpawnedGrid.transform.position;
                    once = true;
                }

                EnemySight.Add(SpawnedGrid);
            }
        }

        foreach (var SpawnedGrid in EnemySight)
        {
            SpawnedGrid.transform.parent = MOSP.transform;
        }

        MOSP.transform.position = this.gameObject.transform.position;
        MOSP.transform.localScale = Vector3.one * 1.5f;
        MOSP.transform.parent = transform;
    }

    private void enemyMovement(Transform Target)
    {
        if (cmty.canMoveThisWay == false)
        {
            return;
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
        if (TargetedGrid != null)
        {
            if (Vector3.Distance(transform.position, TargetedGrid.position) < 0.1f)
            {
                if (MoveNext == true)
                {
                    //CurrPos = transform.position;
                    StartCoroutine(ChangeTargetedGrid(Random.Range(1f, 3f)));
                }
            }

            if (player == null && once2 == false)
            {
                TargetedGrid = EnemyGrid[Random.Range(0, EnemyGrid.Count)].transform;
                once2 = true;
            }
            else
            {
                enemyMovement(TargetedGrid);
            }

        }
    }

    private IEnumerator ChangeTargetedGrid(float delay)
    {
        MoveNext = false;
        Debug.Log("Change targeted grid after " + delay + " seconds");
        yield return new WaitForSeconds(delay);

        Debug.Log("Target Grid Changed");
        TargetedGrid = EnemyGrid[Random.Range(0, EnemyGrid.Count)].transform;

        MoveNext = true;
    }
}
