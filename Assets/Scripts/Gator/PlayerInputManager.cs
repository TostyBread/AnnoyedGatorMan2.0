using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    // References to other components
    private CharacterMovement characterMovement;
    private Fist fist;
    private PlayerPickupSystem playerPickupSystem;
    private PlayerThrowManager playerThrowManager;

    private Vector2 movementInput;

    private bool usableItemModeEnabled = false; // Tracks if usable item mode is active

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
        HandleUsableItemInput();
        HandleStoveInput();

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

        // Cancel throw preparation when RMB is released
        if (Input.GetMouseButtonUp(1))
        {
            playerThrowManager.CancelThrow();
        }
    }

    private void HandleUsableItemInput()
    {
        if (playerPickupSystem == null || !playerPickupSystem.HasItemHeld) return;

        MonoBehaviour usableFunction = playerPickupSystem.GetUsableFunction();

        if (usableFunction == null)
        {
            Debug.Log("No usable function found for the held item.");
            return;
        }

        // Toggle usable item mode with Q key
        if (Input.GetKeyDown(KeyCode.Q))
        {
            usableItemModeEnabled = !usableItemModeEnabled;
            Debug.Log(usableItemModeEnabled ? "Usable item mode enabled" : "Usable item mode disabled");
        }

        // Trigger the usable function on left-click if mode is enabled
        if (usableItemModeEnabled && Input.GetMouseButtonDown(0))
        {
            if (usableFunction is FirearmController firearmController)
            {
                Debug.Log("Attempting to fire firearm...");
                firearmController.Fire();
            }
        }
    }

    private void HandleStoveInput()
    {
        if (Input.GetMouseButtonDown(2)) // Middle-click
        {
            Vector2 mouseWorldPos = ScreenToWorldPointMouse.Instance.GetMouseWorldPosition();
            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);

            if (hit != null)
            {
                CookingStove stove = hit.GetComponent<CookingStove>();
                if (stove != null)
                {
                    stove.ToggleStove();
                }
            }
        }
    }

}