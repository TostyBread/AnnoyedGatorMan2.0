using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPointManager : MonoBehaviour
{
    public bool flying;
    [Header("Ignore if flying is true")]
    public EnemyMovement enemyMovement;

    private bool hasDeactivated = false;

    private void Start()
    {
        StartCoroutine(EnableDetectionAfterStart());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If already deactivated, don't process again
        if (hasDeactivated) return;

        #region Tutorial for ToLower()
        //lets assume collision.name is Walltable
        //If collision.name.ToLower().Cointains("wall"), means that it can get "Wall" & "wall"
        //so it can get name Walltable.
        //While collision.name.Cointains("wall") only can get "wall"
        //so it cannot get name Walltable.
        //it goes the other way for ToUpper().
        #endregion

        // Determine if this grid should be disabled
        bool isObstacle = collision.gameObject.layer == LayerMask.NameToLayer("Obstacle")
                       || collision.name.ToLower().Contains("wall")
                       || collision.name.ToLower().Contains("table"); // <- customize

        if (isObstacle)
        {
            // Deactivate this grid point (to disappear)
            if (flying == false)
            {
                // Remove from enemy's grid/sight list
                enemyMovement.EnemyGrid.Remove(gameObject);

                gameObject.SetActive(false);
            }

            hasDeactivated = true;
        }
    }

    private IEnumerator EnableDetectionAfterStart()
    {
        // Delay to avoid false collision at scene start
        yield return new WaitForSeconds(0.1f);
    }


    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    if (!flying)
    //    {
    //        if (SeeTSightF)
    //        {
    //            if (!enemyMovement.EnemyGrid.Contains(gameObject))
    //            {
    //                enemyMovement.EnemyGrid.Add(gameObject);
    //            }
    //        }
    //        else if (!SeeTSightF)
    //        {
    //            if (!enemyMovement.EnemySight.Contains(gameObject))
    //            {
    //                enemyMovement.EnemySight.Add(gameObject);
    //            }
    //        }
    //    }
    //}
}