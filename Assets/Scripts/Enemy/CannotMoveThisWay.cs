using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannotMoveThisWay : MonoBehaviour
{
    [SerializeField] public bool canMoveThisWay;
    private bool aimForFood;

    private void Start()
    {
        canMoveThisWay = true;
        aimForFood = GetComponentInParent<EnemyMovement>().aimForFood;
    }

    //use OnTrigger and OnCollision to ensure the checking is always correct
    //if not OnTrigger might not get the correct respond(canMoveThisWay)

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !aimForFood)
        { 
            canMoveThisWay = false;

            //Debug.Log("can move this way = " + canMoveThisWay);
            //Debug.Log("collision with = " + collision);
        }

        if (aimForFood && (collision.CompareTag("FoodBig") || collision.CompareTag("FoodSmall")))
        {
            if (collision.gameObject.GetComponent<HealthManager>() != null)
                canMoveThisWay = false;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !aimForFood)
        {
            canMoveThisWay = false;

            //Debug.Log("can move this way = " + canMoveThisWay);
            //Debug.Log("collision with = " + collision);
        }

        if (aimForFood && (collision.gameObject.CompareTag("FoodBig") || collision.gameObject.CompareTag("FoodSmall")))
        {
            if (collision.gameObject.GetComponent<HealthManager>() != null)
                canMoveThisWay = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !aimForFood)
        {
            canMoveThisWay = true;

            //Debug.Log("can move this way = " + canMoveThisWay);
            //Debug.Log("collision with = " + collision);
        }

        if (aimForFood && (collision.CompareTag("FoodBig") || collision.CompareTag("FoodSmall")))
        {
            if (collision.gameObject.GetComponent<HealthManager>() != null)
                canMoveThisWay = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !aimForFood)
        {
            canMoveThisWay = true;

            //Debug.Log("can move this way = " + canMoveThisWay);
            //Debug.Log("collision with = " + collision);
        }

        if (aimForFood && (collision.gameObject.CompareTag("FoodBig") || collision.gameObject.CompareTag("FoodSmall")))
        {
            if (collision.gameObject.GetComponent<HealthManager>() != null)
                canMoveThisWay = true;
        }
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    canMoveThisWay = false;
    //}
}

