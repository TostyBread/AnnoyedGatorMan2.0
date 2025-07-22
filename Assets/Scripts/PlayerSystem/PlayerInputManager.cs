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
    public KeyCode throwKey;

}

public class PlayerInputManager : MonoBehaviour
{
    public enum InputMode { Keyboard }
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

    private float pickupPressTime = 0f;
    private bool isPickupKeyHeld = false;
    private bool pickupHandled = false;
    private const float holdThreshold = 0.15f;

    // References for InteractionPromptUI
    public bool IsPickupKeyHeld() => isPickupKeyHeld;
    public float PickupHoldProgress() => Mathf.Clamp01((Time.time - pickupPressTime) / 0.15f);

    public bool HasHeldItem() => playerPickupSystem != null && playerPickupSystem.HasItemHeld;

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
    }

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

        movementInput = move.normalized;
        characterMovement?.SetMovement(movementInput);
    }

    private void HandleActionInput()
    {
        if (playerPickupSystem == null) return;

        bool used = false;

        if (playerPickupSystem.HasItemHeld)
        {
            IUsable usableFunction = playerPickupSystem.GetUsableFunction();

            switch (usableFunction)
            {
                case FirearmController gun when usableItemModeEnabled:
                    if (gun.currentFireMode == FirearmController.FireMode.Auto)
                    {
                        if (Input.GetKey(inputConfig.attackKey))
                        {
                            gun.Use();
                            used = true;
                        }
                    }
                    else if (Input.GetKeyDown(inputConfig.attackKey))
                    {
                        gun.Use();
                        used = true;
                    }

                    if (Input.GetKeyUp(inputConfig.attackKey))
                        gun.OnFireKeyReleased();
                    break;

                case KnifeController knife when usableItemModeEnabled:
                    if (Input.GetKey(inputConfig.attackKey))
                    {
                        knife.Use();
                        used = true;
                    }
                    break;

                default:
                    // Allow MeleeSwing even if usableItemMode is disabled
                    if (Input.GetKeyDown(inputConfig.attackKey))
                        HandleMeleeLogic();
                    break;
            }

            if (!used && Input.GetKeyDown(inputConfig.attackKey))
            {
                HandleMeleeLogic();
            }
        }
        else
        {
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

        fist?.TriggerPunch();
    }

    private void HandlePickupInput()
    {
        if (playerPickupSystem == null) return;

        if (Input.GetKeyDown(inputConfig.pickupKey))
        {
            pickupPressTime = Time.time;
            isPickupKeyHeld = true;
            pickupHandled = false;
        }

        if (isPickupKeyHeld && !pickupHandled)
        {
            float heldTime = Time.time - pickupPressTime;

            // Optional: Start showing pickup hold progress UI here

            if (heldTime >= holdThreshold)
            {
                playerPickupSystem.StartPickup();

                if (fist != null && fist.isPunching) // Cancel punch if picking up
                    fist.CancelPunch();

                pickupHandled = true;
            }

            // Long interaction support during hold
            playerPickupSystem.StartLongInteraction(true);
        }

        if (Input.GetKeyUp(inputConfig.pickupKey))
        {
            isPickupKeyHeld = false;

            if (!pickupHandled)
            {
                playerPickupSystem.StartInteraction();
            }

            // Always stop long interaction on release
            playerPickupSystem.StartLongInteraction(false);
        }
    }

    private void HandleThrowInput()
    {
        if (!isInputEnabled || playerThrowManager == null || playerPickupSystem == null || !playerPickupSystem.HasItemHeld)
            return;

        if (Input.GetKeyDown(inputConfig.throwKey)) // formerly confirm, now single-button throw
        {
            if (canThrow)
            {
                playerThrowManager.Throw();

                if (fist != null && fist.isPunching) // Cancel punch if throwing
                    fist.CancelPunch();

                usableItemModeEnabled = true; // reset safety if needed
            }
        }
    }

    private void HandleUsableItemInput()
    {
        if (playerPickupSystem == null || !playerPickupSystem.HasItemHeld) return;

        IUsable usableFunction = playerPickupSystem.GetUsableFunction();
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
    }
}