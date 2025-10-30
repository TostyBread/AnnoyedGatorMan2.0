using System;
using System.Collections.Generic;
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
    public int currentTargetIndex; // Index of the current target in the list, use by P3Input's long interaction
    public GameObject CurrentTarget;
    public GameObject Range;
    public List<GameObject> ItemInRange = new List<GameObject>();

    public GameObject P2Player;
    public GameObject HandControl;

    public GameObject Arrow;
    public Vector3 ArrowOffset;

    public Transform HandAim;

    public bool P3;
    public GameObject P3Cursor;
    
    [Header("Input")]
    public KeyCode NextTarget;
    public KeyCode PreviousTarget;

    // Add this property to expose the current aiming direction
    public Vector3 CurrentAimDirection { get; private set; }

    [Header("Cursor Animation")]
    private Animator arrowAnimator;

    P3Controls controls;

    private void OnEnable()
    {
        //if no console detected...
        if (Gamepad.current == null)
        {
            Debug.LogWarning("no console detected");
            //gameObject.SetActive(false);
        }
        else
        {
            controls.Gameplay.Enable();
        }
    }

    private void OnDisable()
    {
        if (Gamepad.current != null)
        {
            controls.Gameplay.Disable();
        }
    }

    private void Awake()
    {
        controls = new P3Controls();

        controls.Gameplay.NextTarget.started += context => NextTargetMethod();
        controls.Gameplay.PreviousTarget.started += context => PreviousTargetMethod();
    }

    // Start is called before the first frame update
    void Start()
    {
        detectTarget = Range.GetComponent<DetectTarget>();

        if (Arrow != null)
            Arrow.transform.parent = null;

        // Cache Arrow's Animator if present
        if (Arrow != null)
            arrowAnimator = Arrow.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

        ChangeTarget();

        //These are for debuging purpose
        CurrentTarget = NearestTarget();
        ItemInRange = detectTarget.AllItemInRange;
        //These are for debuging purpose

        if (NearestTarget() != null)
        {
            //set the arrow to the top of targeted gameobject
            Arrow.transform.position = NearestTarget().transform.position + ArrowOffset;
            HandRotation(NearestTarget().transform.position);
            Arrow.SetActive(true);

            // Ensure animator cached and set parameters when arrow is active
            if (arrowAnimator == null && Arrow != null)
                arrowAnimator = Arrow.GetComponent<Animator>();

            SetArrowAnimatorFlags(true, true);
        }
        else if (NearestTarget() == null) 
        {
            //set weapon that follow the rotation of P3Cursor
            if (P3 && GetComponentInParent<P2PickupSystem>().heldItem != null)
                HandRotation(P3Cursor.transform.position);
            else 
                HandRotation(HandAim.transform.position);

            Arrow.SetActive(false);

            if (arrowAnimator == null && Arrow != null)
                arrowAnimator = Arrow.GetComponent<Animator>();

            SetArrowAnimatorFlags(false, false);
        }

        // Note: P3Cursor is still used for aiming when P3 is enabled; animator for Arrow is handled above.
    }

    // Safely set animator flags only when animator exists, the Arrow is active in hierarchy
    // and the animator actually has the parameter (prevents Unity warnings about missing parameters)
    private void SetArrowAnimatorFlags(bool isInteractable, bool isInRange)
    {
        if (arrowAnimator == null || Arrow == null)
            return;

        // Only set animator params if Arrow is active in hierarchy to avoid some animator warnings in editor/runtime
        if (!Arrow.activeInHierarchy)
            return;

        try
        {
            if (HasAnimatorParameter(arrowAnimator, "IsInteractable"))
                arrowAnimator.SetBool("IsInteractable", isInteractable);
            if (HasAnimatorParameter(arrowAnimator, "IsInRange"))
                arrowAnimator.SetBool("IsInRange", isInRange);
        }
        catch (Exception)
        {
            // Swallow any unexpected animator exceptions to avoid spamming the log
        }
    }

    // Check whether the animator contains a parameter with the given name
    private bool HasAnimatorParameter(Animator animator, string paramName)
    {
        if (animator == null) return false;
        var parameters = animator.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].name == paramName)
                return true;
        }
        return false;
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

    // Add this method to get the effective aiming target position
    public Vector3 GetCurrentAimTarget()
    {
        if (NearestTarget() != null)
        {
            return NearestTarget().transform.position;
        }
        else if (P3 && GetComponentInParent<P2PickupSystem>()?.heldItem != null && P3Cursor != null)
        {
            return P3Cursor.transform.position;
        }
        else
        {
            return HandAim.transform.position;
        }
    }

    private void ChangeTarget()
    {
        if (detectTarget.AllItemInRange.Count > 0)
        {
            if (Input.GetKeyDown(NextTarget)) // Switch to the next enemy  
            {
                NextTargetMethod();
            }

            if (Input.GetKeyDown(PreviousTarget)) // Switch to the previous enemy
            {
                PreviousTargetMethod();
            }
        }
    }

    void NextTargetMethod()
    {
        //int can't be float, so 0.001 still count as 1, except 0
        try
        {
            currentTargetIndex = (currentTargetIndex + 1) % detectTarget.AllItemInRange.Count;
        }
        catch (DivideByZeroException) //might get Divide by 0 error, so remove the error
        { 
        
        }
    }

    void PreviousTargetMethod()
    {
        currentTargetIndex--;
        if (currentTargetIndex < 0)
            currentTargetIndex = detectTarget.AllItemInRange.Count - 1;
    }

    private void HandRotation(Vector3 Target)
    {
        Vector2 direction = Target - P2Player.transform.position;
        CurrentAimDirection = direction; // Store the current aim direction
        
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
