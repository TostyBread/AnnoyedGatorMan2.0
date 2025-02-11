using UnityEngine;

public class PlayerInputManager : MonoBehaviour
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
        movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        characterMovement?.SetMovement(movementInput);
    }

    private void HandleActionInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            fist?.TriggerPunch();
        }
    }

    private void HandlePickupInput()
    {
        if (playerPickupSystem == null) return;

        if (Input.GetKeyDown(KeyCode.E)) playerPickupSystem.StartPickup();
        else if (Input.GetKey(KeyCode.E)) playerPickupSystem.HoldPickup();
        else if (Input.GetKeyUp(KeyCode.E)) playerPickupSystem.CancelPickup();
    }

    private void HandleThrowInput()
    {
        if (playerThrowManager == null || playerPickupSystem == null || !playerPickupSystem.HasItemHeld) return;

        if (Input.GetMouseButtonDown(1)) playerThrowManager.StartPreparingThrow();
        if (Input.GetMouseButtonUp(0) && Input.GetMouseButton(1)) playerThrowManager.Throw();
        if (Input.GetMouseButtonUp(1)) playerThrowManager.CancelThrow();
    }

    private void HandleUsableItemInput()
    {
        if (playerPickupSystem == null || !playerPickupSystem.HasItemHeld) return;

        IUsable usableFunction = playerPickupSystem.GetUsableFunction();
        if (usableFunction == null) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            usableItemModeEnabled = !usableItemModeEnabled;
            Debug.Log(usableItemModeEnabled ? "Usable item mode enabled" : "Usable item mode disabled");
            usableFunction.EnableUsableFunction();

            if (usableFunction is KnifeController knifeController)
            {
                knifeController.ToggleUsableMode(usableItemModeEnabled);
            }
        }

        if (usableItemModeEnabled && Input.GetMouseButtonDown(0))
        {
            usableFunction.Use();
        }
    }

    private void HandleEnvironmentalInteractInput()
    {
        if (Input.GetMouseButtonDown(2))
        {
            playerPickupSystem?.StartInteraction();
        }
    }
}