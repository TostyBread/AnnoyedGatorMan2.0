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
    private bool isPreparingHeld = false;
    private bool throwStarted = false;

    private bool wasFiringLastFrame = false;

    void Awake()
    {
        inputActions = new PlayerInputActions();

        inputActions.Player2Controller.Attack.performed += ctx => HandleActionInput();
        inputActions.Player2Controller.Attack.canceled += ctx => HandleFireKeyReleased();
        inputActions.Player2Controller.Pickup.started += ctx => playerPickupSystemP2?.StartPickup();
        inputActions.Player2Controller.Pickup.performed += ctx => playerPickupSystemP2?.HoldPickup();
        inputActions.Player2Controller.Pickup.canceled += ctx => playerPickupSystemP2?.CancelPickup();
        inputActions.Player2Controller.ThrowPrepare.started += ctx =>
        {
            isPreparingHeld = true;
            TryStartThrow();
        };

        inputActions.Player2Controller.ThrowPrepare.canceled += ctx =>
        {
            isPreparingHeld = false;
            throwStarted = false;
            if (canThrow) playerThrowManagerP2?.CancelThrow();
        };

        inputActions.Player2Controller.ThrowConfirm.started += ctx =>
        {
            if (canThrow && throwStarted) playerThrowManagerP2?.Throw();
        };

        inputActions.Player2Controller.ToggleSafety.performed += ctx => HandleUsableItemInput();
        inputActions.Player2Controller.Interact.performed += ctx => playerPickupSystemP2?.StartInteraction();
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

        if (isPreparingHeld && !throwStarted && canThrow)
            TryStartThrow();
    }

    private void HandleFireModes()
    {
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

    public bool IsPreparingHeld() => isPreparingHeld;

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

    private void TryStartThrow()
    {
        if (!throwStarted && canThrow)
        {
            throwStarted = true;
            playerThrowManagerP2?.StartPreparingThrow();
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