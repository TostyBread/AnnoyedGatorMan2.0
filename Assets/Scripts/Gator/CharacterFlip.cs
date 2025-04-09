using UnityEngine;

public class CharacterFlip : MonoBehaviour
{
    public bool isFacingRight = true; // Tracks the character's facing direction

    void Update()
    {
        HandleFlip();
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