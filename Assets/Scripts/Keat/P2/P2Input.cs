using UnityEngine;

public class P2Input : MonoBehaviour
{
    public bool isInputEnabled = true;
    private CharacterMovement characterMovement;
    private Fist fist;
    private P2PickupSystem p2PickupSystem;
    private PlayerThrowManager playerThrowManager;
    private StateManager stateManager;


    private Vector2 movementInput;
    private bool usableItemModeEnabled = true;
    public bool canThrow = true;
    private bool isPreparingHeld = false;

    private HealthManager healthManager;

    [SerializeField] private float pickupPressTime = 0f;
    [SerializeField] private bool isPickupKeyHeld = false;
    [SerializeField] private bool pickupHandled = false;
    [SerializeField] private const float holdThreshold = 0.15f;

    [Header("Input")]
    public KeyCode Use;
    public KeyCode Throw;
    public KeyCode InterectPick;
    public KeyCode Interact;
    public KeyCode Toggle;

    void Start()
    {
        characterMovement = GetComponent<CharacterMovement>();
        fist = GetComponentInChildren<Fist>();
        p2PickupSystem = GetComponent<P2PickupSystem>();
        playerThrowManager = GetComponent<PlayerThrowManager>();
        healthManager = GetComponent<HealthManager>();
        stateManager = GetComponent<StateManager>();
    }

    void Update()
    {
        HandleMovementInput();
        HandleActionInput(Use);
        HandlePickupInput(InterectPick);
        HandleThrowInput(Throw);
        HandleUsableItemInput(Toggle);
        HandleEnvironmentalInteractInput(Interact);
    }

    public bool IsPreparingHeld() => isPreparingHeld;

    public bool IsUsableModeEnabled() => usableItemModeEnabled;

    private void HandleMovementInput()
    {
        if (stateManager != null && stateManager.state == StateManager.PlayerState.Burn) return;

        if (!healthManager.canMove)
        {
            movementInput = Vector2.zero;
            return;
        }
    }

    private void HandleActionInput(KeyCode a)
    {
        if (p2PickupSystem == null) return;

        bool used = false;

        if (p2PickupSystem.HasItemHeld)
        {
            IUsable usableFunction = p2PickupSystem.GetUsableFunction();

            switch (usableFunction)
            {
                case FirearmController gun when usableItemModeEnabled:
                    if (gun.currentFireMode == FirearmController.FireMode.Auto)
                    {
                        if (Input.GetKey(a))
                        {
                            gun.Use();
                            used = true;
                        }
                    }
                    else if (Input.GetKeyDown(a))
                    {
                        gun.Use();
                        used = true;
                    }

                    if (Input.GetKeyUp(a))
                        gun.OnFireKeyReleased();
                    break;

                case KnifeController knife when usableItemModeEnabled:
                    if (Input.GetKey(a))
                    {
                        knife.Use();
                        used = true;
                    }
                    break;

                default:
                    // Allow MeleeSwing even if usableItemMode is disabled
                    if (Input.GetKeyDown(a))
                        HandleMeleeLogic();
                    break;
            }

            if (!used && Input.GetKeyDown(a))
            {
                HandleMeleeLogic();
            }
        }
        else
        {
            if (Input.GetKeyDown(a))
                fist?.TriggerPunch();
        }
    }

    private void HandleMeleeLogic()
    {
        if (p2PickupSystem != null && p2PickupSystem.HasItemHeld)
        {
            Transform current = p2PickupSystem.GetHeldItem()?.transform;
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

    private void HandlePickupInput(KeyCode a)
    {
        if (p2PickupSystem == null) return;

        if (Input.GetKeyDown(a))
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
                p2PickupSystem.StartPickup();

                if (fist != null && fist.isPunching) // Cancel punch if picking up
                    fist.CancelPunch();

                pickupHandled = true;
            }

            // Long interaction support during hold
            p2PickupSystem.StartLongInteraction(true);
        }

        if (Input.GetKeyUp(a))
        {
            isPickupKeyHeld = false;

            if (!pickupHandled)
            {
                p2PickupSystem.StartInteraction();
            }

            // Always stop long interaction on release
            p2PickupSystem.StartLongInteraction(false);
        }
    }


    private void HandleThrowInput(KeyCode a)
    {
        if (!isInputEnabled || playerThrowManager == null || p2PickupSystem == null || !p2PickupSystem.HasItemHeld)
            return;

        if (Input.GetKeyDown(a)) // formerly confirm, now single-button throw
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

    private void HandleUsableItemInput(KeyCode a)
    {
        if (p2PickupSystem == null || !p2PickupSystem.HasItemHeld) return;

        IUsable usableFunction = p2PickupSystem.GetUsableFunction();
        if (usableFunction == null) return;

        if (Input.GetKeyDown(a))
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

    private void HandleEnvironmentalInteractInput(KeyCode a)
    {
        if (Input.GetKeyDown(a))
        {
            p2PickupSystem?.StartInteraction();
        }
    }
    private void OnDisable()
    {
        movementInput = Vector2.zero;
    }
}

