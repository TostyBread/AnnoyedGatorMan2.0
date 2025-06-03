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
    private P2PickSystem playerPickupSystem;
    private PlayerThrowManager playerThrowManager;
    public Vector2 movementInput;
    private bool usableItemModeEnabled = false;

    P3Controls controls;
    public Vector2 P3move;

    private HealthManager healthManager;
    void Awake()
    {
        characterMovement = GetComponent<CharacterMovement>();
        fist = GetComponentInChildren<Fist>();
        playerPickupSystem = GetComponent<P2PickSystem>();
        playerThrowManager = GetComponent<PlayerThrowManager>();

        controls = new P3Controls();

        controls.Gameplay.Use.performed += context => Use();

        if (playerPickupSystem == null) return;
        else
        {
            controls.Gameplay.Pickup.started += context => StartPickup();
            controls.Gameplay.Pickup.performed += context => HoldPickup();
            controls.Gameplay.Pickup.canceled += context => CancelPickup();
        }

        controls.Gameplay.Move.performed += context => P3move = context.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled += context => P3move = Vector2.zero;

        healthManager = GetComponent<HealthManager>();
    }

    void Use()
    {
        fist?.TriggerPunch();
    }

    void StartPickup()
    {
        playerPickupSystem.StartPickup();
    }

    void HoldPickup()
    { 
        playerPickupSystem.HoldPickup();
    }

    void CancelPickup()
    {
        playerPickupSystem.CancelPickup();
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

    void Update()
    {
        HandleMovementInput();
        //HandleActionInput();
        //HandlePickupInput();
        HandleThrowInput();
        HandleUsableItemInput();
        HandleEnvironmentalInteractInput();
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
        if (playerPickupSystem == null) return;

        if (Input.GetKeyDown(KeyCode.E)) playerPickupSystem.StartPickup();
        else if (Input.GetKey(KeyCode.E)) playerPickupSystem.HoldPickup();
        else if (Input.GetKeyUp(KeyCode.E)) playerPickupSystem.CancelPickup();
    }


    private void HandleThrowInput()
    {
        if (playerThrowManager == null || playerPickupSystem == null || !playerPickupSystem.HasItemHeld) return;

        if (Gamepad.current.buttonSouth.wasPressedThisFrame) playerThrowManager.StartPreparingThrow();
        if (Gamepad.current.buttonWest.wasPressedThisFrame && Gamepad.current.buttonSouth.isPressed) playerThrowManager.Throw();
        if (Gamepad.current.buttonSouth.wasReleasedThisFrame) playerThrowManager.CancelThrow();
    }

    private void HandleUsableItemInput()
    {
        if (playerPickupSystem == null || !playerPickupSystem.HasItemHeld) return;

        IUsable usableFunction = playerPickupSystem.GetUsableFunction();
        if (usableFunction == null) return;

        if (Gamepad.current.rightShoulder.wasPressedThisFrame)
        {
            usableItemModeEnabled = !usableItemModeEnabled;
            Debug.Log(usableItemModeEnabled ? "Usable item mode enabled" : "Usable item mode disabled");
            usableFunction.EnableUsableFunction();

            if (usableFunction is KnifeController knifeController)
            {
                knifeController.ToggleUsableMode(usableItemModeEnabled);
            }
        }

        if (usableItemModeEnabled && Gamepad.current.buttonWest.wasPressedThisFrame|| Gamepad.current.rightTrigger.wasPressedThisFrame)
        {
            usableFunction.Use();
        }
    }

    private void HandleEnvironmentalInteractInput()
    {
        if (Gamepad.current.buttonNorth.wasPressedThisFrame)
        {
            playerPickupSystem?.StartInteraction();
        }
    }
}

