using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class P2PickSystem : MonoBehaviour
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

    [Header("Do not touch")]
    public GameObject Target;
    public GameObject heldItem = null;

    void Update()
    {
        Target = GetComponentInChildren<P2AimSystem>().NearestTarget();

        HandleItemDetection();
        if (isHoldingPickupKey && targetItem != null && pickupCoroutine == null)
        {
            StartPickup();
        }
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

    public void StartInteraction()
    {
        if (targetInteractable != null && targetInteractable.TryGetComponent(out CookingStove stove))
        {
            stove.ToggleStove();
        }
        if (targetInteractable != null && targetInteractable.TryGetComponent(out ItemPackage package))
        {
            package.TakingOutItem();
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
        item.transform.localScale = new Vector3(1, item.transform.localScale.y, item.transform.localScale.z);

        heldItem = item;
        usableItemController = item.GetComponent<IUsable>();
        usableItemController?.EnableUsableFunction();

        if (item.TryGetComponent(out SpriteLayerManager layerManager))
        {
            layerManager.ChangeToHoldingOrder();
        }

        handSpriteManager?.UpdateHandSprite();

        AudioManager.Instance.PlaySound("gunpickup2", 1.0f, transform.position);
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
    public IUsable GetUsableFunction() => usableItemController;

    private void OnDrawGizmosSelected()
    {
        if (Target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, pickupRadius);
        }
    }
}