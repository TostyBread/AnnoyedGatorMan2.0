using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P2Input : MonoBehaviour
{
    private CharacterMovement characterMovement;
    private Fist fist;
    private P2PickSystem P2PickSystem;
    private PlayerThrowManager playerThrowManager;
    private Vector2 movementInput;
    private bool usableItemModeEnabled = false;

    private HealthManager healthManager;

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
        P2PickSystem = GetComponent<P2PickSystem>();
        playerThrowManager = GetComponent<PlayerThrowManager>();
        healthManager = GetComponent<HealthManager>();
    }

    void Update()
    {
        HandleMovementInput();
        HandleActionInput(Use);
        HandlePickupInput(Pick);
        HandleThrowInput(Throw);
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

    private void HandleActionInput(KeyCode a)
    {
        if (Input.GetKeyDown(a))
        {
            fist?.TriggerPunch();
        }
    }

    private void HandlePickupInput(KeyCode a)
    {
        if (P2PickSystem == null) return;

        if (Input.GetKeyDown(a)) P2PickSystem.StartPickup();
        else if (Input.GetKey(a)) P2PickSystem.HoldPickup();
        else if (Input.GetKeyUp(a)) P2PickSystem.CancelPickup();
    }


    private void HandleThrowInput(KeyCode a)
    {
        if (playerThrowManager == null || P2PickSystem == null || !P2PickSystem.HasItemHeld) return;

        if (Input.GetKeyDown(a)) playerThrowManager.StartPreparingThrow();
        if (Input.GetKeyDown(Use) && Input.GetKey(a)) playerThrowManager.Throw();
        if (Input.GetKeyUp(a)) playerThrowManager.CancelThrow();
    }

    private void HandleUsableItemInput(KeyCode a)
    {
        if (P2PickSystem == null || !P2PickSystem.HasItemHeld) return;

        IUsable usableFunction = P2PickSystem.GetUsableFunction();
        if (usableFunction == null) return;

        if (Input.GetKeyDown(a))
        {
            usableItemModeEnabled = !usableItemModeEnabled;
            Debug.Log(usableItemModeEnabled ? "Usable item mode enabled" : "Usable item mode disabled");
            usableFunction.EnableUsableFunction();

            if (usableFunction is KnifeController knifeController)
            {
                knifeController.ToggleUsableMode(usableItemModeEnabled);
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
            P2PickSystem?.StartInteraction();
        }
    }
    private void OnDisable()
    {
        movementInput = Vector2.zero;
    }
}

