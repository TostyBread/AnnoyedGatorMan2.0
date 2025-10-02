using System.Collections.Generic;
using UnityEngine;
using static ItemPackage;

public class PlayerPickupSystemP2 : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float pickupRadius = 2f;
    public Transform handPosition;
    public float dropForce = 5f;
    public List<string> validTags = new List<string>();

    private Collider2D targetItem = null;
    private GameObject heldItem = null;
    private Collider2D targetInteractable = null;

    public HandSpriteManagerP2 handSpriteManagerP2;
    public CharacterFlipP2 characterFlipP2;
    private StateManager stateManager;

    private Window lastWindow;
    private Smoke lastSmoke;

    private IUsable usableItemController;

    public bool HasItemHeld => heldItem != null;
    public string HeldItemTag => heldItem != null ? heldItem.tag : null;
    public bool HasUsableFunction => usableItemController != null;

    private void Start()
    {
        stateManager = GetComponent<StateManager>();
    }

    void Update()
    {
        HandleItemDetection();
    }

    private void HandleItemDetection()
    {
        Vector2 virtualMousPos = PlayerAimController.Instance.GetCursorPosition();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, pickupRadius);

        targetItem = null;
        targetInteractable = null;

        foreach (var collider in colliders)
        {
            bool isPickupable = IsPickupable(collider);
            bool isMouseOver = collider.OverlapPoint(virtualMousPos);
            bool hasInteractable = collider.gameObject.TryGetComponent<Interactable>(out Interactable interactable);

            // If the item is both interactable and pickupable
            if (isPickupable && isMouseOver)
            {
                if (hasInteractable)
                {
                    targetInteractable = collider;
                }
                targetItem = collider;
            }
            else if (hasInteractable && isMouseOver)
            {
                targetInteractable = collider;
            }
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
        }
        else if (lastSmoke != null)
        {
            lastSmoke.SetSmokeState(false, this.gameObject);
            lastSmoke = null;
        }
    }

    public void StartPickup()
    {
        if (targetItem != null)
        {
            if (heldItem != null)
            {
                DropItem();
            }
            PickUpItem(targetItem.gameObject);
        }
    }

    private void PickUpItem(GameObject item)
    {
        PlayerAimController.Instance.ClearLockOn(); // Remove lock on for controller

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
        usableItemController?.EnableUsableFunction();

        if (item.TryGetComponent(out FirearmController firearm)) // When one of the player picks up, it will assign character flip to one of the player
        {
            firearm.SetOwner(gameObject); // assign ownership
        }

        if (item.TryGetComponent(out ItemPackage package)) // Set the owner of the package to the player picking it up
        {
            var detector = GetComponent<NoThrowCursorDetector>(); // Pass the detector to the package for throw validation
            package.SetOwner(PackageOwner.Player2, null, this, null, detector);
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

        handSpriteManagerP2?.UpdateHandSprite();

        AudioManager.Instance.PlaySound("gunpickup2", transform.position);
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

        if (heldItem.TryGetComponent(out ItemPackage package)) // Clear ItemPackage ownership when dropped
        {
            package.ClearOwner();
        }

        bool isFacingRight = characterFlipP2 != null && characterFlipP2.IsFacingRight();
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
            Vector2 virtualMousPos = PlayerAimController.Instance.GetCursorPosition();
            Vector2 direction = (virtualMousPos - (Vector2)dropPosition).normalized;
            rb.AddForce(direction * dropForce, ForceMode2D.Impulse);
        }

        heldItem = null;
        usableItemController = null;
        handSpriteManagerP2?.UpdateHandSprite();
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