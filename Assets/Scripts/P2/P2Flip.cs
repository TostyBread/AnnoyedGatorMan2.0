using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P2Flip : MonoBehaviour
{
    private bool isFacingRight = true; // Tracks the character's facing direction
    private GameObject p2AimingObject;
    public GameObject p2System;
    private bool shouldFaceRight;


    void Update()
    {
        p2AimingObject = p2System.GetComponent<P2AimSystem>().NearestTarget();

        HandleFlip();
    }

    private void HandleFlip()
    {
        // Determine if the character should face right or left
        if (p2AimingObject != null)
        {
            shouldFaceRight = p2AimingObject.transform.position.x >= transform.position.x;
        }
        else
        {
            float a = Input.GetAxis("Horizontal");

            if (a > 0)
            {
                shouldFaceRight = true;

            }
            else if (a < 0)
            {
                shouldFaceRight = false;
            }
        }

        // If the direction changes, flip the character
        if (shouldFaceRight != isFacingRight)
        {
            isFacingRight = shouldFaceRight;
            FlipCharacter();
        }
    }

    private void FlipCharacter()
    {
        // Adjust the X scale to flip the entire character
        Vector3 localScale = transform.localScale;
        localScale.x = isFacingRight ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
        transform.localScale = localScale;
    }

    public bool IsFacingRight()
    {
        return isFacingRight;
    }
}
