using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    private CharacterMovement characterMovement;
    private Fist fist;
    private PlayerPickupSystem playerPickupSystem;
    private PlayerThrowManager playerThrowManager;
    private Vector2 movementInput;
    private bool usableItemModeEnabled = true;

    void Start()
    {
        characterMovement = GetComponent<CharacterMovement>();
        fist = GetComponentInChildren<Fist>();
        playerPickupSystem = GetComponent<PlayerPickupSystem>();
        playerThrowManager = GetComponent<PlayerThrowManager>();
    }

    void Update()
    {
        HandleMovementInput();
        HandleActionInput();
        HandlePickupInput();
        HandleThrowInput();
        HandleUsableItemInput();
        HandleEnvironmentalInteractInput();
    }

    public bool IsUsableModeEnabled() => usableItemModeEnabled;

    private void HandleMovementInput()
    {
        movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        characterMovement?.SetMovement(movementInput);
    }

    private void HandleActionInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (playerPickupSystem != null && playerPickupSystem.HasItemHeld)
            {
                // Traverse hierarchy from heldItem = up to find hand with FistSwing
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

                Debug.LogWarning("No FistSwing found in held item's parent hierarchy.");
            }
            else
            {
                // Use unarmed fist
                fist?.TriggerPunch();
            }
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

        if (Input.GetMouseButtonDown(1)) playerThrowManager.StartPreparingThrow();
        if (Input.GetMouseButtonUp(0) && Input.GetMouseButton(1)) playerThrowManager.Throw();
        if (Input.GetMouseButtonUp(1)) playerThrowManager.CancelThrow();
    }

    private void HandleUsableItemInput()
    {
        if (playerPickupSystem == null || !playerPickupSystem.HasItemHeld) return;

        IUsable usableFunction = playerPickupSystem.GetUsableFunction();
        if (usableFunction == null) return;

        if (Input.GetKeyDown(KeyCode.Q))
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

        if (usableFunction is FirearmController gun && gun.currentFireMode == FirearmController.FireMode.Auto)
        {
            if (Input.GetMouseButton(0))
                usableFunction.Use();
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
                usableFunction.Use();
        }

    }

    private void HandleEnvironmentalInteractInput()
    {
        if (Input.GetMouseButtonDown(2))
        {
            playerPickupSystem?.StartInteraction();
        }
    }
}