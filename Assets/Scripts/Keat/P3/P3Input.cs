using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem;

public class P3Input : MonoBehaviour
{
    private CharacterMovement characterMovement;
    private Fist fist;
    private P2PickupSystem p2PickupSystem;
    private PlayerThrowManager playerThrowManager;
    private Vector2 movementInput;
    private bool usableItemModeEnabled = false;

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

        controls.Gameplay.Use.performed += context => Use();

        controls.Gameplay.Toggle.performed += context => HandleUsableItemInput();
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
        HandleLongInteraction();

        //HandleActionInput();
        //HandlePickupInput();
        //HandleUsableItemInput(Gamepad.current.rightShoulder.wasPressedThisFrame);
        //HandleEnvironmentalInteractInput();
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
        if (Input.GetKeyDown(KeyCode.G))
        {
            fist?.TriggerPunch();
        }
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
        if (playerThrowManager == null || p2PickupSystem == null || !p2PickupSystem.HasItemHeld) return;

        if (Gamepad.current.buttonSouth.wasPressedThisFrame)
            playerThrowManager.StartPreparingThrow();

        if (Gamepad.current.buttonWest.wasPressedThisFrame && Gamepad.current.buttonSouth.isPressed)
        {
            playerThrowManager.Throw();
            usableFunction = null;
            usableItemModeEnabled = false;
        }

        if (Gamepad.current.buttonSouth.wasReleasedThisFrame)
            playerThrowManager.CancelThrow();
    }

    private void HandleUsableItemInput()
    {
        if (p2PickupSystem == null || !p2PickupSystem.HasItemHeld) return;

        usableFunction = p2PickupSystem.GetUsableFunction();
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

    private void HandleEnvironmentalInteractInput()
    {
        if (Gamepad.current.buttonNorth.wasPressedThisFrame)
        {
            p2PickupSystem?.StartInteraction();
        }
    }
}

