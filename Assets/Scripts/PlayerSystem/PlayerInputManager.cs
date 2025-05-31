using UnityEngine;

[System.Serializable]
public class PlayerInputConfig
{
    public KeyCode moveUp;
    public KeyCode moveDown;
    public KeyCode moveLeft;
    public KeyCode moveRight;

    public KeyCode attackKey;
    public KeyCode pickupKey;
    public KeyCode toggleSafetyKey;
    public KeyCode interactKey;
    public KeyCode throwPrepareKey;
    public KeyCode throwConfirmKey;

    public string joystickHorizontalAxis = "P2_LJoystick_Horizontal";
    public string joystickVerticalAxis = "P2_LJoystick_Vertical";
}

public class PlayerInputManager : MonoBehaviour
{
    public enum InputMode { Keyboard, Joystick }
    public InputMode inputMode = InputMode.Keyboard;
    public PlayerInputConfig inputConfig;

    public bool isInputEnabled = true;
    private CharacterMovement characterMovement;
    private Fist fist;
    private PlayerPickupSystem playerPickupSystem;
    private PlayerThrowManager playerThrowManager;
    private StateManager stateManager;

    private Vector2 movementInput;
    private bool usableItemModeEnabled = true;
    public bool canThrow = true;
    private bool isPreparingHeld = false;
    private bool throwStarted = false;

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
        if (!isInputEnabled) return;

        HandleMovementInput();
        HandleActionInput();
        HandlePickupInput();
        HandleThrowInput();
        HandleUsableItemInput();
        HandleEnvironmentalInteractInput();
    }
    public bool IsPreparingHeld() => isPreparingHeld;

    public bool IsUsableModeEnabled() => usableItemModeEnabled;

    private void HandleMovementInput()
    {
        if (stateManager != null && stateManager.state == StateManager.PlayerState.Burn) return;

        Vector2 move = Vector2.zero;

        if (inputMode == InputMode.Keyboard)
        {
            if (Input.GetKey(inputConfig.moveUp)) move.y += 1;
            if (Input.GetKey(inputConfig.moveDown)) move.y -= 1;
            if (Input.GetKey(inputConfig.moveRight)) move.x += 1;
            if (Input.GetKey(inputConfig.moveLeft)) move.x -= 1;
        }
        else if (inputMode == InputMode.Joystick)
        {
            move.x = Input.GetAxis(inputConfig.joystickHorizontalAxis);
            move.y = Input.GetAxis(inputConfig.joystickVerticalAxis);
        }

        movementInput = move.normalized;
        characterMovement?.SetMovement(movementInput);
    }

    private void HandleActionInput()
    {
        if (playerPickupSystem == null) return;

        if (playerPickupSystem.HasItemHeld)
        {
            IUsable usableFunction = playerPickupSystem.GetUsableFunction();
            if (usableFunction == null) return;

            if (usableFunction is FirearmController gun)
            {
                if (gun.currentFireMode == FirearmController.FireMode.Auto)
                {
                    if (Input.GetKey(inputConfig.attackKey))
                        gun.Use();
                }
                else
                {
                    if (Input.GetKeyDown(inputConfig.attackKey))
                        gun.Use();
                }

                if (Input.GetKeyUp(inputConfig.attackKey))
                    gun.OnFireKeyReleased();
            }
            else
            {
                // It's not a gun => fallback to melee
                if (Input.GetKeyDown(inputConfig.attackKey))
                    HandleMeleeLogic();
            }
        }
        else
        {
            // No item held => default to fist
            if (Input.GetKeyDown(inputConfig.attackKey))
                fist?.TriggerPunch();
        }
    }


    private void HandleMeleeLogic()
    {
        if (playerPickupSystem != null && playerPickupSystem.HasItemHeld)
        {
            Transform current = playerPickupSystem.GetHeldItem()?.transform;
            MeleeSwing swing = null;

            while (current != null && swing == null)
            {
                swing = current.GetComponent<MeleeSwing>();
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
        if (playerPickupSystem == null) return;

        if (Input.GetKeyDown(inputConfig.pickupKey)) playerPickupSystem.StartPickup();
        else if (Input.GetKey(inputConfig.pickupKey)) playerPickupSystem.HoldPickup();
        else if (Input.GetKeyUp(inputConfig.pickupKey)) playerPickupSystem.CancelPickup();
    }

    private void HandleThrowInput()
    {
        if (!isInputEnabled || playerThrowManager == null || playerPickupSystem == null || !playerPickupSystem.HasItemHeld)
            return;

        // Begin preparing
        if (Input.GetKeyDown(inputConfig.throwPrepareKey))
        {
            isPreparingHeld = true;
            throwStarted = false;
        }

        // Cancel when letting go of prepare
        if (Input.GetKeyUp(inputConfig.throwPrepareKey))
        {
            isPreparingHeld = false;
            throwStarted = false;
            playerThrowManager.CancelThrow();
            return;
        }

        // Start preparing logic
        if (isPreparingHeld && !throwStarted && canThrow)
        {
            throwStarted = true;
            playerThrowManager.StartPreparingThrow();
        }

        // Final throw condition
        if (Input.GetKeyUp(inputConfig.attackKey))
        {
            if (isPreparingHeld && throwStarted && canThrow)
            {
                playerThrowManager.Throw();

                // RESET STATES after throw
                isPreparingHeld = false;
                throwStarted = false;
            }
            else
            {
                // Always clear throw state if not valid
                throwStarted = false;
            }
        }
    }

    private void HandleUsableItemInput()
    {
        if (playerPickupSystem == null || !playerPickupSystem.HasItemHeld) return;

        IUsable usableFunction = playerPickupSystem.GetUsableFunction();
        if (usableFunction == null) return;

        // Fire input moved to HandleActionInput(), do not repeat it here
        // This block now only toggles usable mode (safety on/off)
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

    }

    private void HandleEnvironmentalInteractInput()
    {
        if (Input.GetKeyDown(inputConfig.interactKey))
        {
            playerPickupSystem?.StartInteraction();
        }

        if (Input.GetKey(inputConfig.interactKey))
        {
            playerPickupSystem?.StartLongInteraction(true);
        }
        else
        {
            playerPickupSystem?.StartLongInteraction(false);
        }
    }
}