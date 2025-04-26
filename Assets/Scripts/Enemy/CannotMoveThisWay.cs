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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        canMoveThisWay = false;

        Debug.Log("can move this way = " + canMoveThisWay);
        Debug.Log("collision with = " + collision);
    }

    //private void OnTriggerStay2D(Collider2D collision)
    //{
    //    canMoveThisWay = false;
    //}

    private void OnTriggerExit2D(Collider2D collision)
    {
        canMoveThisWay = true;

        Debug.Log("can move this way = " + canMoveThisWay);
        Debug.Log("collision with = " + collision);
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    canMoveThisWay = false;
    //}
}

