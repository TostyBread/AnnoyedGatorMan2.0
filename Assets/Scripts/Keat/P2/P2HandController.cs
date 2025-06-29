using UnityEngine;

public class P2HandController : MonoBehaviour
{
    [Header("Pivot Settings")]
    public Transform pivotPoint; // The custom pivot point for hand rotation

    [Header("Hand Settings")]
    public Transform hand; // Reference to the hand object

    [Header("Character Flip Reference")]
    public CharacterFlip characterFlip; // Reference to the CharacterFlip script

    public Transform mousePosition;

    void Update()
    {
        if (pivotPoint != null && hand != null)
        {
            RotateHand();
        }
    }

    private void RotateHand()
    {
        //// Get mouse position in world coordinates
        //Vector3 mousePosition = ScreenToWorldPointMouse.Instance.GetMouseWorldPosition();

        // Calculate the direction from the pivot point to the mouse position
        Vector3 direction = mousePosition.position - pivotPoint.position;

        // Calculate the angle in degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Determine if the character is facing right
        bool isFacingRight = characterFlip != null && characterFlip.IsFacingRight();

        // Adjust the angle based on character's facing direction
        if (!isFacingRight)
        {
            angle += 180f; // Invert the rotation when facing left
        }

        pivotPoint.rotation = Quaternion.Euler(0, 0, angle);

        // Flip the hand sprite correctly based on mouse position
        FlipHand(direction.x, isFacingRight);
    }

    private void FlipHand(float mouseDirectionX, bool isFacingRight)
    {
        Vector3 handScale = hand.localScale;

        // Set the flipped scale
        hand.localScale = handScale;
    }
}
