using System.Collections.Generic;
using UnityEngine;
using static ItemPackage;

public class P2PickupSystem : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float pickupRadius = 2f;
    public Transform handPosition;
    public float dropForce = 5f;
    public List<string> validTags = new List<string>();

    private Collider2D targetItem = null;
    private Collider2D targetInteractable = null;
    private Coroutine pickupCoroutine = null;
    private bool isHoldingPickupKey = false;

    public HandSpriteManager handSpriteManager;
    public CharacterFlip characterFlip;

    private IUsable usableItemController;

    public bool HasItemHeld => heldItem != null;
    public string HeldItemTag => heldItem != null ? heldItem.tag : null;
    public bool HasUsableFunction => usableItemController != null;

    [Header("Interactable Settings")]
    public bool inWindowRange;

    [Header("Do not touch")]
    public GameObject Target;
    public GameObject heldItem = null;
    private StateManager stateManager;

    private Window lastWindow;
    private Smoke lastSmoke;
    public bool isSmoking = false; // for animation to check if player is smoking

    void Start()
    {
        stateManager = GetComponent<StateManager>();
    }

    void Update()
    {
        Target = GetComponentInChildren<P2AimSystem>().NearestTarget();

        HandleItemDetection();
        if (isHoldingPickupKey && targetItem != null && pickupCoroutine == null)
        {
            StartPickup();
        }

        CheckLongInteractionRange();
    }

    private void HandleItemDetection()
    {
        targetItem = null;
        targetInteractable = null;

        if (Target == null) return;

        float distanceToTarget = Vector2.Distance(transform.position, Target.transform.position);
        if (distanceToTarget > pickupRadius) return;

        Collider2D targetCollider = Target.GetComponent<Collider2D>();
        if (targetCollider == null) return;

        if (IsPickupable(targetCollider))
        {
            targetItem = targetCollider;
        }

        if (Target.TryGetComponent(out Interactable interactable))
        {
            targetInteractable = targetCollider;
        }
    }


    private bool IsPickupable(Collider2D collider) => validTags.Contains(collider.tag);

    public void StartInteraction() // Insert anything interactable here, especially communicating with interactable scripts
    {
        if (targetInteractable != null && targetInteractable.TryGetComponent(out CookingStove stove))
        {
            stove.ToggleStove();
        }

        if (targetInteractable != null && targetInteractable.TryGetComponent(out LightSwitch lightSwitch))
        {
            bool inInteractRange = Physics2D.OverlapCircle(transform.position, pickupRadius);
            lightSwitch.ToggleLight(inInteractRange ? stateManager : null);
        }

        if (targetInteractable != null && targetInteractable.TryGetComponent(out ItemPackage package))
        {
            package.TakingOutItem();
        }

        if (targetInteractable != null && targetInteractable.TryGetComponent(out NPCBehavior npc))
        {
            npc.SpawnMenuAndPlate();
        }

        if (targetInteractable != null && targetInteractable.TryGetComponent(out TrashCan trashCan)) // take out trash
        {
            trashCan.TakeOutTrash();
        }
    }

    public void StartLongInteraction(bool isPressed) // Similar to StartInteraction, but require player to keep pressing to interect
    {
        if (targetInteractable != null && targetInteractable.TryGetComponent(out Window window))
        {
            window.SetWindowState(isPressed);
            lastWindow = window;
        }
        else if (lastWindow != null)
        {
            lastWindow.SetWindowState(false);
            lastWindow = null;
        }

        if (targetInteractable != null && targetInteractable.TryGetComponent(out Smoke smoke))
        {
            smoke.SetSmokeState(isPressed, this.gameObject);
            lastSmoke = smoke;

            isSmoking = smoke.isSmoking; // Update smoking state for animation purposes
        }
        else if (lastSmoke != null)
        {
            lastSmoke.SetSmokeState(false, this.gameObject);
            lastSmoke = null;
        }
        else if (lastSmoke == null) isSmoking = false; // Update smoking state for animation purposes
    }

    private void CheckLongInteractionRange()
    {
        inWindowRange = false;

        if (targetInteractable != null && targetInteractable.TryGetComponent(out Window window))
        {
            float distance = Vector2.Distance(transform.position, window.transform.position);
            inWindowRange = distance <= pickupRadius;
        }

        if (!inWindowRange && lastWindow != null)
        {
            lastWindow.SetWindowState(false);
            lastWindow = null;
        }
    }

    public void StartPickup()
    {
        if (targetItem == null) return;

        if (heldItem != null)
        {
            DropItem();
        }

        PickUpItem(targetItem.gameObject);
    }

    private void PickUpItem(GameObject item)
    {
        if (item.TryGetComponent(out PlateSystem plateSystem)) // Specifically letting PlateSystem to know if its being held
            plateSystem.SetHolder(gameObject);

        if (item.TryGetComponent(out Collider2D collider)) collider.enabled = false;

        if (item.TryGetComponent(out Rigidbody2D rb))
        {
            rb.isKinematic = true;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        item.transform.SetParent(handPosition);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.transform.localScale = new Vector3(Mathf.Abs(item.transform.localScale.x), item.transform.localScale.y, item.transform.localScale.z);

        heldItem = item;
        usableItemController = item.GetComponent<IUsable>();
        if (usableItemController != null)
            usableItemController.EnableUsableFunction(); // do not EnableUsableFunction() if the item does not have IUsable component

        if (item.TryGetComponent(out FirearmController firearm)) // When one of the player picks up, it will assign character flip to one of the player
        {
            firearm.SetOwner(gameObject); // assign ownership
        }

        if (item.TryGetComponent(out ItemPackage package)) // Set the owner of the package to the player picking it up
        {
            var detector = GetComponent<NoThrowCursorDetector>(); // Pass the detector to the package for throw validation
            package.SetOwner(PackageOwner.Player3, null, null, this, detector);
        }

        if (item.TryGetComponent(out SpriteLayerManager layerManager))
        {
            layerManager.ChangeToHoldingOrder();
        }

        DamageSource damageSource = item.GetComponentInChildren<DamageSource>(); // Check if the item has a DamageSource component
        if (damageSource != null)
        {
            damageSource.SetOwner(gameObject); // Set the player as owner to ignore self damage and collision
        }

        handSpriteManager?.UpdateHandSprite();

        AudioManager.Instance.PlaySound("gunpickup2", transform.position);

        TrailRenderer trail = heldItem.GetComponentInChildren<TrailRenderer>();
        if (trail != null)
        {
            trail.emitting = false;
        }
    }
    public bool TryManualDrop() // This will be useful incase if other script needed to access and execute drop item
    {
        if (!HasItemHeld) return false;
        DropItem();
        return true;
    }

    public void DropItem()
    {
        if (heldItem == null) return;

        if (heldItem.TryGetComponent(out PlateSystem plateSystem)) // Specifically letting PlateSystem to know if its being dropped (PlateSystem)
            plateSystem.ClearHolder();

        bool isFacingRight = characterFlip != null && characterFlip.IsFacingRight();
        Vector3 dropPosition = handPosition.position + new Vector3(isFacingRight ? -0.2f : 0.5f, -0.5f, 0f);
        heldItem.transform.position = dropPosition;
        if (heldItem.TryGetComponent(out FirearmController firearm)) // When player let go of firearm, it will remove the characterflip associate to that player
        {
            firearm.ClearOwner(); // remove ownership
        }
        heldItem.transform.SetParent(null);

        if (heldItem.TryGetComponent(out SpriteLayerManager layerManager))
        {
            layerManager.RevertToOriginalOrder();
        }
        if (heldItem.TryGetComponent(out Collider2D itemCollider))
        {
            itemCollider.enabled = true;
        }
        if (heldItem.TryGetComponent(out Rigidbody2D rb))
        {
            rb.isKinematic = false;
            Vector2 direction = (ScreenToWorldPointMouse.Instance.GetMouseWorldPosition() - (Vector2)dropPosition).normalized;
            rb.AddForce(direction * dropForce, ForceMode2D.Impulse);
        }

        heldItem = null;
        usableItemController = null;
        handSpriteManager?.UpdateHandSprite();
    }

    public GameObject GetHeldItem() => heldItem;
    public IUsable GetUsableFunction()
    {
        return usableItemController;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}