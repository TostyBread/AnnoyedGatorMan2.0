using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class PlayerPickupSystem : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float pickupRadius = 2f;
    public Transform handPosition;
    public float dropForce = 5f;
    public List<string> validTags = new List<string>();

    private Collider2D targetItem = null;
    private GameObject heldItem = null;
    private Collider2D targetInteractable = null;
    private Coroutine pickupCoroutine = null;
    private bool isHoldingPickupKey = false;

    public HandSpriteManager handSpriteManager;
    public CharacterFlip characterFlip;
    private StateManager stateManager;

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
        if (isHoldingPickupKey && targetItem != null && pickupCoroutine == null)
        {
            StartPickup();
        }
    }

    private void HandleItemDetection()
    {
        Vector2 mouseWorldPos = ScreenToWorldPointMouse.Instance.GetMouseWorldPosition();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, pickupRadius);

        targetItem = null;
        targetInteractable = null;

        foreach (var collider in colliders)
        {
            bool isPickupable = IsPickupable(collider);
            bool isMouseOver = collider.OverlapPoint(mouseWorldPos);
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

        if (targetInteractable != null && targetInteractable.TryGetComponent(out Window window))
        {
            window.ToogleWindow();
        }

        if (targetInteractable != null && targetInteractable.TryGetComponent(out ItemPackage package))
        {
            package.TakingOutItem();
        }
        if (targetInteractable != null && targetInteractable.TryGetComponent(out NPCBehavior npc))
        {
            npc.SpawnMenuAndPlate();
        }
    }

    public void StartPickup()
    {
        if (targetItem != null && pickupCoroutine == null)
        {
            pickupCoroutine = StartCoroutine(PickupItemCoroutine());
        }
    }

    public void HoldPickup()
    {
        isHoldingPickupKey = true;
        if (targetItem != null && pickupCoroutine == null)
        {
            StartPickup();
        }
    }

    public void CancelPickup()
    {
        isHoldingPickupKey = false;
        if (pickupCoroutine != null)
        {
            StopCoroutine(pickupCoroutine);
            pickupCoroutine = null;
        }
    }

    private IEnumerator PickupItemCoroutine()
    {
        float holdTime = 0.2f;
        float elapsedTime = 0f;

        while (elapsedTime < holdTime)
        {
            if (targetItem == null || !isHoldingPickupKey) yield break;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (targetItem != null)
        {
            if (heldItem != null)
            {
                DropItem();
            }
            PickUpItem(targetItem.gameObject);
        }
        pickupCoroutine = null;
    }

    private void PickUpItem(GameObject item)
    {
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
        item.transform.localScale = new Vector3(1, item.transform.localScale.y, item.transform.localScale.z);

        heldItem = item;
        usableItemController = item.GetComponent<IUsable>();
        usableItemController?.EnableUsableFunction();

        if (item.TryGetComponent(out FirearmController firearm)) // When one of the player picks up, it will assign character flip to one of the player
        {
            firearm.SetOwner(gameObject); // assign ownership
        }

        if (item.TryGetComponent(out SpriteLayerManager layerManager))
        {
            layerManager.ChangeToHoldingOrder();
        }

        handSpriteManager?.UpdateHandSprite();

        AudioManager.Instance.PlaySound("gunpickup2", 1.0f, transform.position);
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
