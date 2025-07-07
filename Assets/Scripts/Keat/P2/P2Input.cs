using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class P2Input : MonoBehaviour
{
    private CharacterMovement characterMovement;
    private Fist fist;
    private P2PickupSystem p2PickupSystem;
    private PlayerThrowManager playerThrowManager;
    private Vector2 movementInput;
    private bool usableItemModeEnabled = true;
    public bool canThrow = true;
    private bool isPreparingHeld = false;
    private bool throwStarted = false;

    private HealthManager healthManager;
    private PlayerInputManager playerInputManager;

    [Header("Input")]
    public KeyCode Use;
    public KeyCode Throw;
    public KeyCode Pick;
    public KeyCode Interact;
    public KeyCode Toggle;

    void Start()
    {
        characterMovement = GetComponent<CharacterMovement>();
        fist = GetComponentInChildren<Fist>();
        p2PickupSystem = GetComponent<P2PickupSystem>();
        playerThrowManager = GetComponent<PlayerThrowManager>();
        healthManager = GetComponent<HealthManager>();

        playerInputManager = GetComponent<PlayerInputManager>();
    }

    void Update()
    {

        if (playerInputManager != null)
        canThrow = playerInputManager.canThrow;

        HandleMovementInput();
        HandleActionInput(Use);
        HandlePickupInput(Pick);
        HandleThrowInput(Throw,Use);
        HandleUsableItemInput(Toggle);
        HandleEnvironmentalInteractInput(Interact);
    }

    public bool IsUsableModeEnabled() => usableItemModeEnabled;

    private void HandleMovementInput()
    {
        if (!healthManager.canMove)
        {
            movementInput = Vector2.zero;
            return;
        }
        movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        characterMovement?.SetMovement(movementInput);
    }

    private void HandleActionInput(KeyCode use)
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
                        if (Input.GetKey(use))
                        {
                            gun.Use();
                            used = true;
                        }
                    }
                    else if (Input.GetKeyDown(use))
                    {
                        gun.Use();
                        used = true;
                    }

                    if (Input.GetKeyUp(use))
                        gun.OnFireKeyReleased();
                    break;

                case KnifeController knife when usableItemModeEnabled:
                    if (Input.GetKey(use))
                    {
                        knife.Use();
                        used = true;
                    }
                    break;

                default:
                    // Allow MeleeSwing even if usableItemMode is disabled
                    if (Input.GetKeyDown(use))
                        HandleMeleeLogic();
                    break;
            }

            if (!used && Input.GetKeyDown(use))
            {
                HandleMeleeLogic();
            }
        }
        else
        {
            if (Input.GetKeyDown(use))
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

        if (Input.GetKeyDown(a)) p2PickupSystem.StartPickup();
        else if (Input.GetKey(a)) p2PickupSystem.HoldPickup();
        else if (Input.GetKeyUp(a)) p2PickupSystem.CancelPickup();
    }


    private void HandleThrowInput(KeyCode PrepareToThrow,KeyCode Use)
    {
        if (playerThrowManager == null || p2PickupSystem == null || !p2PickupSystem.HasItemHeld)
            return;

        if (Input.GetKeyDown(PrepareToThrow))
        {
            isPreparingHeld = true;
            throwStarted = false;
        }

        if (Input.GetKeyUp(PrepareToThrow))
        {
            isPreparingHeld = false;
            throwStarted = false;
            playerThrowManager.CancelThrow();
            return;
        }

        if (isPreparingHeld && !throwStarted && canThrow)
        {
            throwStarted = true;
            playerThrowManager.StartPreparingThrow();
        }

        if (Input.GetKeyUp(Use))
        {
            if (isPreparingHeld && throwStarted && canThrow)
            {
                playerThrowManager.Throw();
                isPreparingHeld = false;
                throwStarted = false;

                // Reset usable mode after throw
                usableItemModeEnabled = true;
            }
            else
            {
                throwStarted = false;
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

        if (usableItemModeEnabled && Input.GetKeyDown(Use))
        {
            usableFunction.Use();
        }
    }

    private void HandleEnvironmentalInteractInput(KeyCode a)
    {
        if (Input.GetKeyDown(a))
        {
            p2PickupSystem?.StartInteraction();
        }

        if (Input.GetKey(a))
        {
            p2PickupSystem?.StartLongInteraction(true);
        }
        else
        {
            p2PickupSystem?.StartLongInteraction(false);
        }
    }
    private void OnDisable()
    {
        movementInput = Vector2.zero;
    }
}

