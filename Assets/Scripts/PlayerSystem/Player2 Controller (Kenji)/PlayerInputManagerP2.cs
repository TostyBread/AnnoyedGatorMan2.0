using UnityEngine;

public class PlayerInputManagerP2 : MonoBehaviour
{
    private PlayerInputActions inputActions;
    private CharacterMovement characterMovement;
    private Fist fist;
    private PlayerPickupSystemP2 playerPickupSystemP2;
    private PlayerThrowManagerP2 playerThrowManagerP2;
    private StateManager stateManager;

    private Vector2 movementInput;
    private bool usableItemModeEnabled = true;
    public bool isInputEnabled = true;
    public bool canThrow = true;
    private bool isPickupKeyHeld = false;
    private bool pickupHandled = false;

    private bool wasFiringLastFrame = false;
    private float pickupPressTime = 0f;
    private const float holdThreshold = 0.15f;

    public bool HasHeldItem() => playerPickupSystemP2 != null && playerPickupSystemP2.HasItemHeld;

    void Awake()
    {
        inputActions = new PlayerInputActions();

        inputActions.Player2Controller.Pickup.started += ctx =>
        {
            pickupPressTime = Time.time;
            isPickupKeyHeld = true;
            pickupHandled = false;
        };

        inputActions.Player2Controller.Pickup.canceled += ctx =>
        {
            isPickupKeyHeld = false;

            if (!pickupHandled)
            {
                playerPickupSystemP2?.StartInteraction();
            }
        };

        inputActions.Player2Controller.Attack.performed += ctx => HandleActionInput();
        inputActions.Player2Controller.Attack.canceled += ctx => HandleFireKeyReleased();

        inputActions.Player2Controller.Throw.started += ctx =>
        {
            if (canThrow)
            {
                playerThrowManagerP2?.Throw();

                if (fist != null && fist.isPunching) // Cancel punch if throwing
                    fist.CancelPunch();

                usableItemModeEnabled = true; // reset after throw
            }
        };


        inputActions.Player2Controller.ToggleSafety.performed += ctx => HandleUsableItemInput();
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

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
        HandleFireModes();
        HandleKnife();

        if (isPickupKeyHeld && !pickupHandled) // Keep checking the input held elapsed time
        {
            float heldTime = Time.time - pickupPressTime;
            if (heldTime >= holdThreshold)
            {
                playerPickupSystemP2?.StartPickup();

                if (fist != null && fist.isPunching) // Cancel punch if picking up
                    fist.CancelPunch();

                pickupHandled = true;
            }
        }

        // Always stop long interaction on release
        playerPickupSystemP2.StartLongInteraction(isPickupKeyHeld);
    }

    private void HandleFireModes()
    {
        if (!usableItemModeEnabled) return;
        if (playerPickupSystemP2 == null || !playerPickupSystemP2.HasItemHeld) return;

        IUsable usable = playerPickupSystemP2.GetUsableFunction();
        if (usable == null) return;

        bool isPressed = inputActions.Player2Controller.Attack.ReadValue<float>() > 0.5f;

        if (usable is FirearmController firearm)
        {
            if (firearm.currentFireMode == FirearmController.FireMode.Auto)
            {
                if (isPressed) firearm.Use();
            }
            else
            {
                if (isPressed && !wasFiringLastFrame) firearm.Use();
            }
        }

        wasFiringLastFrame = isPressed;
    }

    private void HandleKnife()
    {
        if (!usableItemModeEnabled) return;
        if (playerPickupSystemP2 == null || !playerPickupSystemP2.HasItemHeld) return;

        IUsable usable = playerPickupSystemP2.GetUsableFunction();
        if (usable == null) return;

        if (usable is KnifeController knife)
        {
            if (inputActions.Player2Controller.Attack.ReadValue<float>() > 0.5f)
                knife.Use();
        }
    }

    private void HandleMovementInput()
    {
        if (stateManager != null && stateManager.state == StateManager.PlayerState.Burn) return;

        movementInput = inputActions.Player2Controller.Movement.ReadValue<Vector2>().normalized;
        characterMovement?.SetMovement(movementInput);
    }

    private void HandleActionInput()
    {
        if (playerPickupSystemP2 == null) return;

        bool used = false;

        if (playerPickupSystemP2.HasItemHeld)
        {
            IUsable usableFunction = playerPickupSystemP2.GetUsableFunction();

            switch (usableFunction)
            {
                case FirearmController gun when usableItemModeEnabled:
                    bool isPressed = inputActions.Player2Controller.Attack.ReadValue<float>() > 0.5f;
                    if (gun.currentFireMode == FirearmController.FireMode.Auto)
                    {
                        if (isPressed)
                        {
                            gun.Use();
                            used = true;
                        }
                    }
                    else
                    {
                        if (isPressed && !wasFiringLastFrame)
                        {
                            gun.Use();
                            used = true;
                        }
                    }
                    if (!isPressed && wasFiringLastFrame)
                    {
                        gun.OnFireKeyReleased();
                    }
                    wasFiringLastFrame = isPressed;
                    break;

                case KnifeController knife when usableItemModeEnabled:
                    if (inputActions.Player2Controller.Attack.ReadValue<float>() > 0.5f)
                    {
                        knife.Use();
                        used = true;
                    }
                    break;

                case ItemPackage package when usableItemModeEnabled:
                    if (inputActions.Player2Controller.Attack.WasPressedThisFrame())
                    {
                        package.Use();
                        used = true;
                    }
                    break;

                default:
                    // Melee logic (MeleeSwingP2)
                    Transform current = playerPickupSystemP2.GetHeldItem()?.transform;
                    MeleeSwingP2 swing = null;
                    while (current != null && swing == null)
                    {
                        swing = current.GetComponent<MeleeSwingP2>();
                        current = current.parent;
                    }
                    if (swing != null && inputActions.Player2Controller.Attack.WasPressedThisFrame())
                    {
                        swing.Use();
                        used = true;
                    }
                    break;
            }

            // If not used, fallback to melee punch
            if (!used && inputActions.Player2Controller.Attack.WasPressedThisFrame())
            {
                fist?.TriggerPunch();
            }
        }
        else
        {
            if (inputActions.Player2Controller.Attack.WasPressedThisFrame())
                fist?.TriggerPunch();
        }
    }

    private void HandleUsableItemInput()
    {
        if (playerPickupSystemP2 == null || !playerPickupSystemP2.HasItemHeld) return;

        IUsable usableFunction = playerPickupSystemP2.GetUsableFunction();
        if (usableFunction == null) return;

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
                // Note: ItemPackage does not have a toggleable mode
        }
    }

    private void HandleFireKeyReleased()
    {
        IUsable usableFunction = playerPickupSystemP2?.GetUsableFunction();
        if (usableFunction is FirearmController firearm)
        {
            firearm.OnFireKeyReleased();
        }
    }
}