using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    // References to other components
    private CharacterMovement characterMovement;
    private Fist fist;
    private PlayerPickupSystem playerPickupSystem;

    private Vector2 movementInput;

    void Start()
    {
        // Get references to the required components
        characterMovement = GetComponent<CharacterMovement>();
        fist = GetComponentInChildren<Fist>(); // Assuming Fist is a child object
        playerPickupSystem = GetComponent<PlayerPickupSystem>(); // Pickup system on the same GameObject
    }

    void Update()
    {
        HandleMovementInput();
        HandleActionInput();
        HandlePickupInput();
    }

    private void HandleMovementInput()
    {
        // Get movement input
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");

        // Normalize movement vector and send it to CharacterMovement
        movementInput = movementInput.normalized;
        if (characterMovement != null)
        {
            characterMovement.SetMovement(movementInput);
        }
    }

    private void HandleActionInput()
    {
        // Handle punching input
        if (Input.GetMouseButtonDown(0))
        {
            if (fist != null)
            {
                fist.TriggerPunch();
            }
        }
    }

    private void HandlePickupInput()
    {
        if (playerPickupSystem == null) return;

        // Handle E key input
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Start a new pickup process
            playerPickupSystem.StartPickup();
        }
        else if (Input.GetKey(KeyCode.E))
        {
            // Continue pickup while the E key is held
            playerPickupSystem.HoldPickup();
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            // Cancel the pickup process when the E key is released
            playerPickupSystem.CancelPickup();
        }
    }
}