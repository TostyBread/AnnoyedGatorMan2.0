using UnityEngine;

public class GridPointManager : MonoBehaviour
{
    public bool flying;
    [Header("Ignore if flying is true")]
    public bool SeeTSightF;
    public EnemyMovement enemyMovement;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!flying || flying && collision.name.Contains("Walls"))
        {
            //try remove and add enemyMovement[] 
            if (SeeTSightF)
            {
                enemyMovement.EnemyGrid.Remove(gameObject);
            }
            else if (!SeeTSightF)
            {
                enemyMovement.EnemySight.Remove(gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!flying)
        {
            if (SeeTSightF)
            {
                if (!enemyMovement.EnemyGrid.Contains(gameObject))
                {
                    enemyMovement.EnemyGrid.Add(gameObject);
                }
            }
            else if (!SeeTSightF)
            {
                if (!enemyMovement.EnemySight.Contains(gameObject))
                {
                    enemyMovement.EnemySight.Add(gameObject);
                }
            }
        }
    }

}