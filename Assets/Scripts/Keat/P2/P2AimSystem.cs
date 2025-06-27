using UnityEngine;
using UnityEngine.InputSystem;

public class P2AimSystem : MonoBehaviour
{

    [Header("Pivot Settings")]
    public Transform pivotPoint; // The custom pivot point for hand rotation

    [Header("Hand Settings")]
    public Transform hand; // Reference to the hand object

    [Header("Character Flip Reference")]
    public CharacterFlip characterFlip; // Reference to the CharacterFlip script


    private DetectTarget detectTarget;
    private int currentTargetIndex;
    public GameObject Range;

    public GameObject P2Player;
    private GameObject CurrentTarget;
    public GameObject HandControl;

    public GameObject Arrow;
    public Vector3 ArrowOffset;

    public Transform HandAim;

    public bool P3;
    public GameObject P3Cursor;
    [Header("Input")]
    public KeyCode NextTarget;
    public KeyCode PreviousTarget;


    // Start is called before the first frame update
    void Start()
    {
        detectTarget = Range.GetComponent<DetectTarget>();

        Arrow.transform.parent = null;
    }

    // Update is called once per frame
    void Update()
    {
        ChangeTarget();
        CurrentTarget = NearestTarget();

        if (NearestTarget() != null)
        {
            //set the arrow to the top of targeted gameobject
            Arrow.transform.position = NearestTarget().transform.position + ArrowOffset;
            HandRotation(NearestTarget().transform.position);
            Arrow.SetActive(true);
        }
        else if (NearestTarget() == null) 
        {
            //set weapon that follow the rotation of P3Cursor
            if (P3 && GetComponentInParent<P2PickSystem>().heldItem != null)
                HandRotation(P3Cursor.transform.position);
            else 
                HandRotation(HandAim.transform.position);

            Arrow.SetActive(false);
        }
    }

    public GameObject NearestTarget()
    {
        if (detectTarget.AllItemInRange.Count == 0)
        {
            return null;
        }


        //Ensure index is within bounds
        currentTargetIndex = Mathf.Clamp(currentTargetIndex, 0, detectTarget.AllItemInRange.Count - 1);
        

        return detectTarget.AllItemInRange[currentTargetIndex];
    }

    private void ChangeTarget()
    {
        if (detectTarget.AllItemInRange.Count > 0)
        {
            if (Gamepad.current == null)
                return;     

            if (Input.GetKeyDown(NextTarget) || P3 && Gamepad.current.leftTrigger.wasPressedThisFrame) // Switch to the next enemy  
            {
                //int can't be float, so 0.001 still count as 1, except 0
                currentTargetIndex = (currentTargetIndex + 1) % detectTarget.AllItemInRange.Count;
            }

            if (Input.GetKeyDown(PreviousTarget) || P3 && Gamepad.current.leftShoulder.wasPressedThisFrame) // Switch to the previous enemy
            {
                currentTargetIndex--;
                if (currentTargetIndex < 0)
                    currentTargetIndex = detectTarget.AllItemInRange.Count - 1;
            }
        }
    }

    private void HandRotation(Vector3 Target)
    {
        Vector2 direction = Target - P2Player.transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        hand.transform.rotation = Quaternion.Euler(0, 0, angle);

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
