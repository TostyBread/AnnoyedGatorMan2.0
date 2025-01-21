using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    // References to other components
    private CharacterMovement characterMovement;
    private Fist fist;
    private PlayerPickupSystem playerPickupSystem;
    private PlayerThrowManager playerThrowManager;

    private Vector2 movementInput;

    void Start()
    {
        // Get references to the required components
        characterMovement = GetComponent<CharacterMovement>();
        fist = GetComponentInChildren<Fist>(); // Assuming Fist is a child object
        playerPickupSystem = GetComponent<PlayerPickupSystem>(); // Pickup system on the same GameObject
        playerThrowManager = GetComponent<PlayerThrowManager>(); // Throw manager on the same GameObject
    }

    void Update()
    {
        HandleMovementInput();
        HandleActionInput();
        HandlePickupInput();
        HandleThrowInput();
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
            playerPickupSystem.StartPickup();
        }
        else if (Input.GetKey(KeyCode.E))
        {
            playerPickupSystem.HoldPickup();
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            playerPickupSystem.CancelPickup();
        }
    }

    private void HandleThrowInput()
    {
        if (playerThrowManager == null || playerPickupSystem == null || !playerPickupSystem.HasItemHeld) return;

        // Start preparing to throw when RMB is pressed
        if (Input.GetMouseButtonDown(1))
        {
            playerThrowManager.StartPreparingThrow();
        }

        // Throw the item when LMB is clicked while RMB is held
        if (Input.GetMouseButtonUp(0) && Input.GetMouseButton(1))
        {
            playerThrowManager.Throw();
        }
    }
}