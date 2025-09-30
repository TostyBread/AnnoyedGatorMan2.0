using UnityEngine;
using UnityEngine.InputSystem;

public class P3Input : MonoBehaviour
{
    public bool isInputEnabled = true;
    private CharacterMovement characterMovement;
    private Fist fist;
    private P2PickupSystem p2PickupSystem;
    private PlayerThrowManager playerThrowManager;

    private Vector2 movementInput;
    private bool usableItemModeEnabled = true;
    public bool canThrow = true;

    P3Controls controls;
    public Vector2 P3move;

    private HealthManager healthManager;
    IUsable usableFunction;

    [SerializeField] private bool wasFiringLastFrame = false;

    [SerializeField] private float pickupPressTime = 0f;
    [SerializeField] private bool isPickupKeyHeld = false;
    [SerializeField] private bool pickupHandled = false;
    [SerializeField] private const float holdThreshold = 0.15f;

    void Awake()
    {
        characterMovement = GetComponent<CharacterMovement>();
        fist = GetComponentInChildren<Fist>();
        p2PickupSystem = GetComponent<P2PickupSystem>();
        playerThrowManager = GetComponent<PlayerThrowManager>();

        controls = new P3Controls();

        if (!isInputEnabled) return;

        // Use performed instead of started for single-press actions
        controls.Gameplay.Use.performed += context => HandleUseInput();
        controls.Gameplay.Use.canceled += context => HandleUseRelease();

        controls.Gameplay.Toggle.performed += context => HandleUsableItemInput();
        controls.Gameplay.Throw.performed += context => HandleThrowInput();

        if (p2PickupSystem != null)
        {
            controls.Gameplay.Pickup.started += context => OnPickupStarted();
            controls.Gameplay.Pickup.canceled += context => OnPickupCanceled();
        }

        controls.Gameplay.Move.performed += context => P3move = context.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled += context => P3move = Vector2.zero;

        healthManager = GetComponent<HealthManager>();
    }

    private void HandleUseInput()
    {
        if (p2PickupSystem == null) return;

        bool used = false;

        if (p2PickupSystem.HasItemHeld)
        {
            usableFunction = p2PickupSystem.GetUsableFunction();

            switch (usableFunction)
            {
                case FirearmController gun when usableItemModeEnabled:
                    bool isPressed = controls.Gameplay.Use.ReadValue<float>() > 0.5f;
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
                    if (controls.Gameplay.Use.ReadValue<float>() > 0.5f)
                    {
                        knife.Use();
                        used = true;
                    }
                    break;

                case ItemPackage package when usableItemModeEnabled:
                    if (controls.Gameplay.Use.WasPressedThisFrame())
                    {
                        package.Use();
                        used = true;
                    }
                    break;

                default:
                    // Melee logic
                    if (controls.Gameplay.Use.WasPressedThisFrame())
                    {
                        HandleMeleeLogic();
                        used = true;
                    }
                    break;
            }

            // If not used, fallback to melee punch
            if (!used && controls.Gameplay.Use.WasPressedThisFrame())
            {
                HandleMeleeLogic();
            }
        }
        else
        {
            if (controls.Gameplay.Use.WasPressedThisFrame())
                fist?.TriggerPunch();
        }
    }

    private void HandleFireModes()
    {
        if (!usableItemModeEnabled) return;
        if (p2PickupSystem == null || !p2PickupSystem.HasItemHeld) return;

        IUsable usable = p2PickupSystem.GetUsableFunction();
        if (usable == null) return;

        bool isPressed = controls.Gameplay.Use.ReadValue<float>() > 0.5f;

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
        if (p2PickupSystem == null || !p2PickupSystem.HasItemHeld) return;

        IUsable usable = p2PickupSystem.GetUsableFunction();
        if (usable == null) return;

        if (usable is KnifeController knife)
        {
            if (controls.Gameplay.Use.ReadValue<float>() > 0.5f)
                knife.Use();
        }
    }

    private void HandleUseRelease()
    {
        IUsable usableFunction = p2PickupSystem?.GetUsableFunction();
        if (usableFunction is FirearmController firearm)
        {
            firearm.OnFireKeyReleased();
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

    private void OnPickupStarted()
    {
        if (p2PickupSystem == null) return;

        pickupPressTime = Time.time;
        isPickupKeyHeld = true;
        pickupHandled = false;
    }

    private void HandlePickupHold()
    {
        if (p2PickupSystem == null || !isPickupKeyHeld || pickupHandled)
            return;

        float heldTime = Time.time - pickupPressTime;

        if (heldTime >= holdThreshold)
        {
            p2PickupSystem.StartPickup();

            if (fist != null && fist.isPunching)
                fist.CancelPunch();

            pickupHandled = true;
        }

        // Enable long interaction feedback during hold
        p2PickupSystem.StartLongInteraction(true);
    }

    private void OnPickupCanceled()
    {
        if (p2PickupSystem == null) return;

        isPickupKeyHeld = false;

        if (!pickupHandled)
        {
            // Treat as interaction if not held long enough
            p2PickupSystem.StartInteraction();
        }

        // Always stop long interaction on release
        p2PickupSystem.StartLongInteraction(false);
    }

    private void OnEnable()
    {
        //if no console detected...
        if (Gamepad.current == null)
        {
            Debug.LogWarning("no console detected");
            //gameObject.SetActive(false);
        }
        else
        {
            controls.Gameplay.Enable();
        }
    }

    private void OnDisable()
    {
        movementInput = Vector2.zero;

        if (Gamepad.current != null)
        {
            controls.Gameplay.Disable();
        }
    }

    void EnableInput()
    {
        controls.Gameplay.Enable();
    }

    void DisableInput()
    {
        controls.Gameplay.Disable();
    }

    void Start()
    {
        characterMovement = GetComponent<CharacterMovement>();
        fist = GetComponentInChildren<Fist>();
        p2PickupSystem = GetComponent<P2PickupSystem>();
        playerThrowManager = GetComponent<PlayerThrowManager>();
        healthManager = GetComponent<HealthManager>();
    }

    void Update()
    {
        if (isInputEnabled && Gamepad.current != null)
            EnableInput();
        else
            DisableInput();

        if (!isInputEnabled) return;

        HandleMovementInput();
        HandleFireModes();
        HandleKnife();
        HandlePickupHold();
    }

    public bool IsUsableModeEnabled() => usableItemModeEnabled;

    private void HandleMovementInput()
    {
        if (!healthManager.canMove)
        {
            movementInput = Vector2.zero;
            return;
        }
        movementInput = new Vector2(P3move.x, P3move.y).normalized;

        characterMovement?.SetMovement(movementInput);
    }

    private void HandleThrowInput()
    {
        if (!isInputEnabled || playerThrowManager == null || p2PickupSystem == null || !p2PickupSystem.HasItemHeld)
            return;

        if (canThrow)
        {
            playerThrowManager.Throw();

            if (fist != null && fist.isPunching) // Cancel punch if throwing
                fist.CancelPunch();

            usableItemModeEnabled = true; // reset safety if needed
        }
    }

    private void HandleUsableItemInput()
    {
        if (p2PickupSystem == null || !p2PickupSystem.HasItemHeld) return;

        usableFunction = p2PickupSystem.GetUsableFunction();
        if (usableFunction == null) return;

        usableItemModeEnabled = !usableItemModeEnabled;
        Debug.Log(usableItemModeEnabled ? "Usable item mode enabled" : "Usable item mode disabled");
        usableFunction.EnableUsableFunction();

        if (usableFunction is KnifeController knifeController)
        {
            knifeController.ToggleUsableMode(usableItemModeEnabled);
        }
        if (usableFunction is FirearmController firearmController)
        {
            firearmController.ToggleUsableMode(usableItemModeEnabled);
        }
    }
}

