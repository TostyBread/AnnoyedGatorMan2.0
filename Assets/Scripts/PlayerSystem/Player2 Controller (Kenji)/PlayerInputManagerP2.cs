using UnityEngine;

[System.Serializable]
public class PlayerInputConfigP2
{
    public string joystickHorizontalAxis = "P2_LJoystick_Horizontal";
    public string joystickVerticalAxis = "P2_LJoystick_Vertical";

    public KeyCode attackKey;
    public KeyCode pickupKey;
    public KeyCode toggleSafetyKey;
    public KeyCode interactKey;
    public KeyCode throwPrepareKey;
    public KeyCode throwConfirmKey;
}

public class PlayerInputManagerP2 : MonoBehaviour
{
    public PlayerInputConfigP2 inputConfig;

    public bool isInputEnabled = true;
    private CharacterMovement characterMovement;
    private Fist fist;
    private PlayerPickupSystemP2 playerPickupSystemP2;
    private PlayerThrowManagerP2 playerThrowManagerP2;
    private StateManager stateManager;

    private Vector2 movementInput;
    private bool usableItemModeEnabled = true;

    void Start()
    {
        characterMovement = GetComponent<CharacterMovement>();
        fist = GetComponentInChildren<Fist>();
        playerPickupSystemP2 = GetComponent<PlayerPickupSystemP2>();
        playerThrowManagerP2 = GetComponent<PlayerThrowManagerP2>();
        stateManager = GetComponent<StateManager>();
    }

    void Update()
    {
        if (!isInputEnabled) return;

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
        if (stateManager != null && stateManager.state == StateManager.PlayerState.Burn) return;

        Vector2 move = Vector2.zero;
        move.x = Input.GetAxis(inputConfig.joystickHorizontalAxis);
        move.y = Input.GetAxis(inputConfig.joystickVerticalAxis);

        movementInput = move.normalized;
        characterMovement?.SetMovement(movementInput);
    }

    private void HandleActionInput()
    {
        if (Input.GetKeyDown(inputConfig.attackKey)) HandleMeleeLogic();
    }

    private void HandleMeleeLogic()
    {
        if (playerPickupSystemP2 != null && playerPickupSystemP2.HasItemHeld)
        {
            Transform current = playerPickupSystemP2.GetHeldItem()?.transform;
            MeleeSwingP2 swing = null;

            while (current != null && swing == null)
            {
                swing = current.GetComponent<MeleeSwingP2>();
                current = current.parent;
            }

            if (swing != null)
            {
                swing.Use();
                return;
            }
        }
        else
        {
            fist?.TriggerPunch();
        }
    }

    private void HandlePickupInput()
    {
        if (playerPickupSystemP2 == null) return;

        if (Input.GetKeyDown(inputConfig.pickupKey)) playerPickupSystemP2.StartPickup();
        else if (Input.GetKey(inputConfig.pickupKey)) playerPickupSystemP2.HoldPickup();
        else if (Input.GetKeyUp(inputConfig.pickupKey)) playerPickupSystemP2.CancelPickup();
    }

    private void HandleThrowInput()
    {
        if (playerThrowManagerP2 == null || playerPickupSystemP2 == null || !playerPickupSystemP2.HasItemHeld) return;

        if (Input.GetKeyDown(inputConfig.throwPrepareKey)) playerThrowManagerP2.StartPreparingThrow();
        if (Input.GetKeyUp(inputConfig.attackKey) && Input.GetKey(inputConfig.throwPrepareKey)) playerThrowManagerP2.Throw();
        if (Input.GetKeyUp(inputConfig.throwPrepareKey)) playerThrowManagerP2.CancelThrow();
    }

    private void HandleUsableItemInput()
    {
        if (playerPickupSystemP2 == null || !playerPickupSystemP2.HasItemHeld) return;

        IUsable usableFunction = playerPickupSystemP2.GetUsableFunction();
        if (usableFunction == null) return;

        if (Input.GetKeyDown(inputConfig.toggleSafetyKey))
        {
            usableItemModeEnabled = !usableItemModeEnabled;
            Debug.Log(usableItemModeEnabled ? "Usable item mode enabled" : "Usable item mode disabled");
            usableFunction.EnableUsableFunction();

            switch (usableFunction)
            {
                case KnifeController knife:
                    knife.ToggleUsableMode(usableItemModeEnabled);
                    break;
                case FirearmController firearm:
                    firearm.ToggleUsableMode(usableItemModeEnabled);
                    break;
            }
        }

        if (usableFunction is FirearmController gun && gun.currentFireMode == FirearmController.FireMode.Auto)
        {
            if (Input.GetKeyDown(inputConfig.attackKey))
                usableFunction.Use();
        }
        else
        {
            if (Input.GetKeyDown(inputConfig.attackKey))
                usableFunction.Use();
        }
    }

    private void HandleEnvironmentalInteractInput()
    {
        if (Input.GetKeyDown(inputConfig.interactKey))
        {
            playerPickupSystemP2?.StartInteraction();
        }
    }
}