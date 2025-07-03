using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    public bool inInterectRange;

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

        SetInInterectRange();
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

    private void SetInInterectRange()
    {
        if (targetInteractable != null && targetInteractable.TryGetComponent(out Interactable interactable))
        {
            inInterectRange = true;
        }
        else
        {
            inInterectRange = false;
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
        float holdTime = 0.3f;
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
        item.transform.localScale = new Vector3(Mathf.Abs(item.transform.localScale.x), item.transform.localScale.y, item.transform.localScale.z);

        heldItem = item;
        usableItemController = item.GetComponent<IUsable>();
        if (usableItemController != null)
            usableItemController.EnableUsableFunction(); // do not EnableUsableFunction() if the item does not have IUsable component

        if (item.TryGetComponent(out FirearmController firearm)) // When one of the player picks up, it will assign character flip to one of the player
        {
            firearm.SetOwner(gameObject); // assign ownership
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
    }

    public void DropItem(bool applyForce = true)
    {
        if (heldItem == null) return;

        bool isFacingRight = characterFlip != null && characterFlip.IsFacingRight();
        Vector3 dropPosition = handPosition.position + new Vector3(isFacingRight ? -0.2f : 0.5f, -0.5f, 0f);
        heldItem.transform.position = dropPosition;
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

            if (applyForce)
            {
                Vector2 direction = ((Vector2)Target.transform.position - (Vector2)dropPosition).normalized;
                if (direction == Vector2.zero)
                {
                    direction = isFacingRight ? Vector2.right : Vector2.left;
                }

                rb.AddForce(direction * dropForce, ForceMode2D.Impulse);
            }
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
        if (Target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, pickupRadius);
        }
    }
}