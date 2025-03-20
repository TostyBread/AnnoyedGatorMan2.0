using UnityEngine;

public class Player2InputManager : MonoBehaviour
{
    private CharacterMovement characterMovement;
    private Fist fist;
    private PlayerPickupSystem playerPickupSystem;
    private PlayerThrowManager playerThrowManager;
    private Vector2 movementInput;
    private bool usableItemModeEnabled = false;

    void Start()
    {
        characterMovement = GetComponent<CharacterMovement>();
        fist = GetComponentInChildren<Fist>();
        playerPickupSystem = GetComponent<PlayerPickupSystem>();
        playerThrowManager = GetComponent<PlayerThrowManager>();
    }

    void Update()
    {
        HandleMovementInput();
        HandleActionInput();
        HandlePickupInput();
        HandleThrowInput();
        HandleUsableItemInput();
        HandleEnvironmentalInteractInput();
    }

    public bool IsUsableModeEnabled() => usableItemModeEnabled;

    private void HandleMovementInput()
    {
        movementInput = new Vector2(Input.GetAxisRaw("Horizontal2"), Input.GetAxisRaw("Vertical2")).normalized;
        characterMovement?.SetMovement(movementInput);
    }

    private void HandleActionInput()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            fist?.TriggerPunch();
        }
    }

    private void HandlePickupInput()
    {
        if (playerPickupSystem == null) return;

        if (Input.GetKeyDown(KeyCode.Keypad1)) playerPickupSystem.StartPickup();
        else if (Input.GetKey(KeyCode.Keypad1)) playerPickupSystem.HoldPickup();
        else if (Input.GetKeyUp(KeyCode.Keypad1)) playerPickupSystem.CancelPickup();
    }

    private void HandleThrowInput()
    {
        if (playerThrowManager == null || playerPickupSystem == null || !playerPickupSystem.HasItemHeld) return;

        if (Input.GetKeyDown(KeyCode.Keypad1)) playerThrowManager.StartPreparingThrow();
        if (Input.GetKeyUp(KeyCode.Keypad0) && Input.GetKey(KeyCode.Keypad1)) playerThrowManager.Throw();
        if (Input.GetKeyUp(KeyCode.Keypad1)) playerThrowManager.CancelThrow();
    }

    private void HandleUsableItemInput()
    {
        if (playerPickupSystem == null || !playerPickupSystem.HasItemHeld) return;

        IUsable usableFunction = playerPickupSystem.GetUsableFunction();
        if (usableFunction == null) return;

        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            usableItemModeEnabled = !usableItemModeEnabled;
            Debug.Log(usableItemModeEnabled ? "Usable item mode enabled" : "Usable item mode disabled");
            usableFunction.EnableUsableFunction();

            if (usableFunction is KnifeController knifeController)
            {
                knifeController.ToggleUsableMode(usableItemModeEnabled);
            }
        }

        if (usableItemModeEnabled && Input.GetKeyDown(KeyCode.Keypad0))
        {
            usableFunction.Use();
        }
    }

    private void HandleEnvironmentalInteractInput()
    {
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            playerPickupSystem?.StartInteraction();
        }
    }
}
