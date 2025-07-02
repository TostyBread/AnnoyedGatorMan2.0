using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class P3Input : MonoBehaviour
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

    P3Controls controls;
    public Vector2 P3move;

    private HealthManager healthManager;
    IUsable usableFunction;

    void Awake()
    {
        characterMovement = GetComponent<CharacterMovement>();
        fist = GetComponentInChildren<Fist>();
        p2PickupSystem = GetComponent<P2PickupSystem>();
        playerThrowManager = GetComponent<PlayerThrowManager>();

        controls = new P3Controls();

        //controls.Gameplay.Use.performed += context => Use();

        //controls.Gameplay.Toggle.performed += context => HandleUsableItemInput();
        controls.Gameplay.Throw.performed += context => HandleThrowInput();


        if (p2PickupSystem == null) return;
        else
        {
            controls.Gameplay.Pickup.started += context => StartPickup();
            controls.Gameplay.Pickup.performed += context => HoldPickup();
            controls.Gameplay.Pickup.canceled += context => CancelPickup();

            controls.Gameplay.Interect.performed += context => p2PickupSystem.StartInteraction();
        }

        controls.Gameplay.Move.performed += context => P3move = context.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled += context => P3move = Vector2.zero;

        
        healthManager = GetComponent<HealthManager>();
    }

    void Use()
    {
        if (usableItemModeEnabled && p2PickupSystem != null && p2PickupSystem.HasItemHeld)
        {
            usableFunction = p2PickupSystem.GetUsableFunction();
            usableFunction?.Use();
        }
        else
        {
            fist?.TriggerPunch();
        }
    }

    void StartPickup()
    {
        p2PickupSystem.StartPickup();
    }

    void HoldPickup()
    { 
        p2PickupSystem.HoldPickup();
    }

    void CancelPickup()
    {
        p2PickupSystem.CancelPickup();
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
        HandleMovementInput();
        HandleThrowInput();
        HandleActionInput();
        HandleLongInteraction();
        HandleUsableItemInput();

        //HandleActionInput();
        //HandlePickupInput();
        //HandleEnvironmentalInteractInput();
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

    private void HandleActionInput()
    {
        if (p2PickupSystem == null) return;

        bool used = false;

        if (p2PickupSystem.HasItemHeld && Gamepad.current != null)
        {
            IUsable usableFunction = p2PickupSystem.GetUsableFunction();

            switch (usableFunction)
            {
                case FirearmController gun when usableItemModeEnabled:
                    if (gun.currentFireMode == FirearmController.FireMode.Auto)
                    {
                        if (Gamepad.current.buttonWest.isPressed)
                        {
                            gun.Use();
                            used = true;
                        }
                    }
                    else if (Gamepad.current.buttonWest.wasPressedThisFrame)
                    {
                        gun.Use();
                        used = true;
                    }

                    if (Gamepad.current.buttonWest.wasReleasedThisFrame)
                        gun.OnFireKeyReleased();
                    break;

                case KnifeController knife when usableItemModeEnabled:
                    if (Gamepad.current.buttonWest.isPressed)
                    {
                        knife.Use();
                        used = true;
                    }
                    break;

                default:
                    // Allow MeleeSwing even if usableItemMode is disabled
                    if (Gamepad.current.buttonWest.wasPressedThisFrame)
                        HandleMeleeLogic();
                    break;
            }

            if (!used && Gamepad.current.buttonWest.wasPressedThisFrame)
            {
                HandleMeleeLogic();
            }
        }
        //else
        //{
        //    if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame)
        //        fist?.TriggerPunch();
        //}

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

    private void HandlePickupInput()
    {
        if (p2PickupSystem == null) return;

        if (Input.GetKeyDown(KeyCode.E)) p2PickupSystem.StartPickup();
        else if (Input.GetKey(KeyCode.E)) p2PickupSystem.HoldPickup();
        else if (Input.GetKeyUp(KeyCode.E)) p2PickupSystem.CancelPickup();
    }


    private void HandleThrowInput()
    {
        if (playerThrowManager == null || p2PickupSystem == null || !p2PickupSystem.HasItemHeld)
            return;

        if (Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            isPreparingHeld = true;
            throwStarted = false;
        }

        if (Gamepad.current.buttonSouth.wasReleasedThisFrame)
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

        if (Gamepad.current.buttonWest.wasReleasedThisFrame)
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


    private void HandleUsableItemInput()
    {
        if (p2PickupSystem == null || !p2PickupSystem.HasItemHeld) return;

        IUsable usableFunction = p2PickupSystem.GetUsableFunction();
        if (usableFunction == null) return;

        if (Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame)
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

        if (usableItemModeEnabled && Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            usableFunction.Use();
        }
    }

    private void HandleEnvironmentalInteractInput()
    {
        if (Gamepad.current.buttonNorth.wasPressedThisFrame)
        {
            p2PickupSystem?.StartInteraction();
        }
    }
    private void HandleLongInteraction()
    {
        if (p2PickupSystem.Target)
        {
            controls.Gameplay.Interect.performed += context => p2PickupSystem.StartLongInteraction(true);
            controls.Gameplay.Interect.canceled += context => p2PickupSystem.StartLongInteraction(false);
        }
        else
        {
            p2PickupSystem?.StartLongInteraction(false);
        }
    }

}

