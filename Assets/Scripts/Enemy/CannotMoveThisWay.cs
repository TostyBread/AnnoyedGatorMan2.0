using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannotMoveThisWay : MonoBehaviour
{
    [SerializeField] public bool canMoveThisWay;
    private void Start()
    {
        canMoveThisWay = true;
    }

    //use OnTrigger and OnCollision to ensure the checking is always correct
    //if not OnTrigger might not get the correct respond(canMoveThisWay)

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        { 
            canMoveThisWay = false;

            Debug.Log("can move this way = " + canMoveThisWay);
            Debug.Log("collision with = " + collision);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            canMoveThisWay = false;

            Debug.Log("can move this way = " + canMoveThisWay);
            Debug.Log("collision with = " + collision);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canMoveThisWay = true;

            Debug.Log("can move this way = " + canMoveThisWay);
            Debug.Log("collision with = " + collision);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            canMoveThisWay = true;

            Debug.Log("can move this way = " + canMoveThisWay);
            Debug.Log("collision with = " + collision);
        }
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    canMoveThisWay = false;
    //}
}

