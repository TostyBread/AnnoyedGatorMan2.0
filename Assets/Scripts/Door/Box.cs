using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    private DoorManager doorManager;
    private void Start()
    {
        doorManager = GetComponentInParent<DoorManager>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) 
        {
            Fist fist = collision.gameObject.GetComponentInChildren<Fist>();

            if (fist != null)
            {
                if (fist.isPunching)
                    doorManager.PlayerAtTheRightPos = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            doorManager.PlayerAtTheRightPos = false;
        }
    }
}
