using UnityEngine;

public class CharacterFlip : MonoBehaviour
{
    private bool isFacingRight = true; // Tracks the character's facing direction
    private GameObject p2AimingObject;
    public GameObject p2System;
    private bool shouldFaceRight;

    void Update()
    {
        if (p2System != null)
        {
            p2AimingObject = p2System.GetComponent<P2AimSystem>().NearestTarget();
            P2HandleFlip();
        }
        else
        {
            HandleFlip();
        }
    }

    private void HandleFlip()
    {
        Vector3 mousePosition = ScreenToWorldPointMouse.Instance.GetMouseWorldPosition();

        // Determine if the character should face right or left
        bool shouldFaceRight = mousePosition.x >= transform.position.x;

        // If the direction changes, flip the character
        if (shouldFaceRight != isFacingRight)
        {
            isFacingRight = shouldFaceRight;
            FlipCharacter();
        }
    }

    private void P2HandleFlip()
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