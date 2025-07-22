using UnityEngine;
using UnityEngine.InputSystem;
using static FirearmController;

public class CharacterFlip : MonoBehaviour, ICharacterFlip
{
    [Header("Facing Control")]
    public bool isFacingRight = true; // Tracks the character's facing direction
    public bool isFlippingEnabled = true;

    [Header("P2 / Alt Control Settings")]
    public bool useP2System = false;
    public GameObject p2System;
    public bool WASDControl;
    private GameObject p2AimingObject;
    public P3Input p3Input;

    private bool shouldFaceRight;

    private void Update()
    {
        if (!isFlippingEnabled) return;

        if (useP2System && p2System != null)
        {
            p2AimingObject = p2System.GetComponent<P2AimSystem>()?.NearestTarget();
            HandleP2Flip();
        }
        else
        {
            HandleMouseFlip();
        }
    }

    private void HandleMouseFlip()
    {
        Vector3 mousePosition = ScreenToWorldPointMouse.Instance.GetMouseWorldPosition();
        bool shouldFaceRight = mousePosition.x >= transform.position.x;

        if (shouldFaceRight != isFacingRight)
        {
            isFacingRight = shouldFaceRight;
            FlipCharacter();
        }
    }

    private void HandleP2Flip()
    {
        if (p2AimingObject != null)
        {
            shouldFaceRight = p2AimingObject.transform.position.x >= transform.position.x;
        }
        else
        {
            float x = 0f;

            if (p3Input != null)
                x = p3Input.P3move.x;
            else if (WASDControl) //WASD is "Horizontal" || UpDownLeftRight is "Horizontal2"
                x = Input.GetAxisRaw("Horizontal");
            else
                x = Input.GetAxisRaw("Horizontal2");

            if (x > 0)
                shouldFaceRight = true;
            else if (x < 0)
                shouldFaceRight = false;
        }

        if (shouldFaceRight != isFacingRight)
        {
            isFacingRight = shouldFaceRight;
            FlipCharacter();
        }
    }

    private void FlipCharacter()
    {
        Vector3 localScale = transform.localScale;

        localScale.x = isFacingRight ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);

        transform.localScale = localScale;
    }

    public bool IsFacingRight() => isFacingRight;
}