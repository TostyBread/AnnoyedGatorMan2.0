using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyEffect : MonoBehaviour
{
    public Transform Target; //This variable is set by EnemySpawner
    [SerializeField] private float waitfor;
    private NavMeshAgent agent;
    private bool arriveTarget;
    [SerializeField]private EnemySpawner enemySpawner;

    // Start is called before the first frame update
    void Start()
    {
        arriveTarget = false;
        agent = GetComponent<NavMeshAgent>();
        enemySpawner = FindAnyObjectByType<EnemySpawner>();

        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning("GameObject PathFinder is missing");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Target == null) return;

        agent.SetDestination(Target.position);
        Rotation(Target.transform);

        if (Vector2.Distance(transform.position, Target.position) < 0.1f) arriveTarget = true;

        if (arriveTarget == true)
        {
            StartCoroutine(WaitBeforeDestroy(waitfor));
        }
        
    }

    private void Rotation(Transform target)
    {
        if (target == null || Vector2.Distance(transform.position,target.transform.position) < 0.1f) return;

        Vector2 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion Rotation = Quaternion.Euler(0, 0, angle);
        gameObject.transform.rotation = Rotation;
    }

    IEnumerator WaitBeforeDestroy(float second)
    {
        yield return new WaitForSeconds(second);
        GameObject enemy = Instantiate(enemySpawner.EnemyForCurrentWeather, transform.position, transform.rotation); //lets spawn enemy


        arriveTarget = false;
        Destroy(gameObject);
    }
}
