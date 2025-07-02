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