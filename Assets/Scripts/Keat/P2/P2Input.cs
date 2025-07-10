using UnityEngine;

public class P2Input : MonoBehaviour
{
    private CharacterMovement characterMovement;
    private Fist fist;
    private P2PickupSystem p2PickupSystem;
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
        p2PickupSystem = GetComponent<P2PickupSystem>();
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
        if (p2PickupSystem == null) return;

        if (Input.GetKeyDown(a)) p2PickupSystem.StartPickup();
        else if (Input.GetKey(a)) p2PickupSystem.HoldPickup();
        else if (Input.GetKeyUp(a)) p2PickupSystem.CancelPickup();
    }


    private void HandleThrowInput(KeyCode a)
    {
        if (playerThrowManager == null || p2PickupSystem == null || !p2PickupSystem.HasItemHeld) return;

        //if (Input.GetKeyDown(a)) playerThrowManager.StartPreparingThrow();  // Nots using it anymore
        if (Input.GetKeyDown(Use) && Input.GetKey(a)) playerThrowManager.Throw();
        //if (Input.GetKeyUp(a)) playerThrowManager.CancelThrow(); // Not using it anymore
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
            p2PickupSystem?.StartInteraction();
        }
    }
    private void OnDisable()
    {
        movementInput = Vector2.zero;
    }
}

