using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    private CharacterMovement characterMovement;
    private Fist fist;
    private PlayerPickupSystem playerPickupSystem;
    private PlayerThrowManager playerThrowManager;
    private Vector2 movementInput;
    private bool usableItemModeEnabled = false;

    public bool Player2;
    private StateManager stateManager;

    void Start()
    {
        characterMovement = GetComponent<CharacterMovement>();
        fist = GetComponentInChildren<Fist>();
        playerPickupSystem = GetComponent<PlayerPickupSystem>();
        playerThrowManager = GetComponent<PlayerThrowManager>();

        stateManager = GetComponent<StateManager>();
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
        if (stateManager != null && stateManager.state == StateManager.PlayerState.Burn)
        {
            return;
        }

        if (!Player2) movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (Player2) movementInput = new Vector2(Input.GetAxisRaw("Horizontal2"), Input.GetAxisRaw("Vertical2")).normalized;
        characterMovement?.SetMovement(movementInput);
    }

    private void HandleActionInput()
    {
        if (Input.GetMouseButtonDown(0) && !Player2)
        {
            fist?.TriggerPunch();
        }

        if (Input.GetKeyDown(KeyCode.Keypad0) && Player2)
        {
            fist?.TriggerPunch();
        }
    }

    private void HandlePickupInput()
    {
        if (playerPickupSystem == null) return;

        if (!Player2)
        {
            if (Input.GetKeyDown(KeyCode.E)) playerPickupSystem.StartPickup();
            else if (Input.GetKey(KeyCode.E)) playerPickupSystem.HoldPickup();
            else if (Input.GetKeyUp(KeyCode.E)) playerPickupSystem.CancelPickup();
        }

        if (Player2)
        {
            if (Input.GetKeyDown(KeyCode.Keypad1)) playerPickupSystem.StartPickup();
            else if (Input.GetKey(KeyCode.Keypad1)) playerPickupSystem.HoldPickup();
            else if (Input.GetKeyUp(KeyCode.Keypad1)) playerPickupSystem.CancelPickup();
        }
    }

    private void HandleThrowInput()
    {
        if (playerThrowManager == null || playerPickupSystem == null || !playerPickupSystem.HasItemHeld) return;

        if (!Player2)
        {
            if (Input.GetMouseButtonDown(1)) playerThrowManager.StartPreparingThrow();
            if (Input.GetMouseButtonUp(0) && Input.GetMouseButton(1)) playerThrowManager.Throw();
            if (Input.GetMouseButtonUp(1)) playerThrowManager.CancelThrow();
        }

        if (Player2)
        {
            if (Input.GetKeyDown(KeyCode.Keypad1)) playerThrowManager.StartPreparingThrow();
            if (Input.GetKeyUp(KeyCode.Keypad0) && Input.GetKey(KeyCode.Keypad1)) playerThrowManager.Throw();
            if (Input.GetKeyUp(KeyCode.Keypad1)) playerThrowManager.CancelThrow();
        }
    }

    private void HandleUsableItemInput()
    {
        if (playerPickupSystem == null || !playerPickupSystem.HasItemHeld) return;

        IUsable usableFunction = playerPickupSystem.GetUsableFunction();
        if (usableFunction == null) return;

        if (Input.GetKeyDown(KeyCode.Q) && !Player2 || Input.GetKeyDown(KeyCode.Keypad2) && Player2)
        {
            usableItemModeEnabled = !usableItemModeEnabled;
            Debug.Log(usableItemModeEnabled ? "Usable item mode enabled" : "Usable item mode disabled");
            usableFunction.EnableUsableFunction();

            if (usableFunction is KnifeController knifeController)
            {
                knifeController.ToggleUsableMode(usableItemModeEnabled);
            }
        }

        if (usableItemModeEnabled && Input.GetMouseButtonDown(0) && !Player2)
        {
            usableFunction.Use();
        }

        if (usableItemModeEnabled && Input.GetKeyDown(KeyCode.Keypad0) && Player2)
        {
            usableFunction.Use();
        }
    }

    private void HandleEnvironmentalInteractInput()
    {
        if (Input.GetMouseButtonDown(2) && !Player2)
        {
            playerPickupSystem?.StartInteraction();
        }

        if (Input.GetKeyDown(KeyCode.Keypad3) && Player2)
        {
            playerPickupSystem?.StartInteraction();
        }
    }
}