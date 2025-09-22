using UnityEngine;
using System.Collections;

public class ItemPackage : MonoBehaviour, IUsable
{
    public enum PackageOwner { None, Player1, Player2 }
    private PackageOwner owner = PackageOwner.None;

    [Header("Spawn Settings")]
    public GameObject itemPrefab;
    public int itemCount;
    public bool destroyWhenEmpty = false;
    public bool canThrowItems = false;
    public Transform spawnPosition;
    public GameObject originalSprite;
    public GameObject emptySprite;
    private int itemAvailability;

    [Header("Throw Settings")]
    public float throwSpeed = 18f;
    public float spinSpeed = 900f;

    private bool usableModeActive = true;
    private bool isBeingDestroyed = false;

    private ScreenToWorldPointMouse p1Cursor; // K&B
    private PlayerAimController p2Cursor; // Kenji's old controller
    private PlayerPickupSystem p1PickupSystem; // K&B
    private PlayerPickupSystemP2 p2PickupSystem; // Kenji's old controller 
    private NoThrowCursorDetector throwBlockDetector; // Detect whether the player can throw or not

    private void Awake()
    {
        if (emptySprite != null)
        {
            originalSprite.SetActive(true);
            emptySprite.SetActive(false);
        }
        itemAvailability = 0;
    }

    public void TakingOutItem()
    {
        if (itemPrefab != null && spawnPosition != null)
        {
            if (itemAvailability != itemCount)
            {
                Instantiate(itemPrefab, spawnPosition.position, spawnPosition.rotation);
                ++itemAvailability;
                PackageEmpty();
                return;
            }
        }
    }

    public void SetOwner(PackageOwner newOwner, PlayerPickupSystem p1System = null, PlayerPickupSystemP2 p2System = null, NoThrowCursorDetector detector = null)
    {
        owner = newOwner;
        p1PickupSystem = p1System;
        p2PickupSystem = p2System;
        throwBlockDetector = detector;

        if (owner == PackageOwner.Player1)
            p1Cursor = ScreenToWorldPointMouse.Instance;
        else if (owner == PackageOwner.Player2)
            p2Cursor = PlayerAimController.Instance;
    }

    // IUsable implementation
    public void EnableUsableFunction()
    {
        usableModeActive = true;
    }

    public void DisableUsableFunction()
    {
        usableModeActive = false;
    }

    public bool IsInUsableMode()
    {
        return usableModeActive;
    }

    // Called by PlayerInputManager when attackKey is pressed
    public void Use()
    {
        if (!usableModeActive) return;
        if (canThrowItems)
        {
            ThrowItem();
        }
    }

    private void ThrowItem()
    {
        if (itemPrefab != null && itemAvailability != itemCount && !isBeingDestroyed)
        {
            // Check throw blocking
            if (throwBlockDetector != null)
            {
                bool canThrow = owner switch
                {
                    PackageOwner.Player1 => throwBlockDetector.InputManager != null && throwBlockDetector.InputManager.canThrow,
                    PackageOwner.Player2 => throwBlockDetector.InputManagerP2 != null && throwBlockDetector.InputManagerP2.canThrow,
                    _ => true
                };
                if (!canThrow) return; // Block throw if not allowed
            }

            GameObject thrownItem = Instantiate(itemPrefab, transform.position, Quaternion.identity);
            Vector2 targetPos = owner switch
            {
                PackageOwner.Player1 when p1Cursor != null => p1Cursor.GetMouseWorldPosition(),
                PackageOwner.Player2 when p2Cursor != null => p2Cursor.GetCursorPosition(),
                _ => transform.position
            };

            Rigidbody2D rb = thrownItem.GetComponent<Rigidbody2D>();
            Collider2D collider = thrownItem.GetComponent<Collider2D>();

            if (rb) rb.isKinematic = true;
            if (collider) collider.enabled = false;

            ++itemAvailability;
            bool isLastItem = itemAvailability == itemCount;
            StartCoroutine(SimulateThrow(thrownItem, targetPos, collider, isLastItem));

            if (isLastItem)
            {
                if (!destroyWhenEmpty)
                {
                    originalSprite.SetActive(false);
                    emptySprite.SetActive(true);
                }
                isBeingDestroyed = true;
            }
        }
    }

    private IEnumerator SimulateThrow(GameObject item, Vector2 targetPos, Collider2D itemCollider, bool isLastItem)
    {
        Vector2 startPos = item.transform.position;
        float distance = Vector2.Distance(startPos, targetPos);
        float duration = distance / throwSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = 1 - Mathf.Pow(1 - t, 3);

            item.transform.position = Vector2.Lerp(startPos, targetPos, easedT);
            item.transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime * (targetPos.x > startPos.x ? -1 : 1));

            yield return null;
        }

        item.transform.position = targetPos;

        if (itemCollider != null)
            itemCollider.enabled = true;

        if (item.TryGetComponent(out Rigidbody2D rb))
            rb.isKinematic = false;

        // Only destroy after the last item's throw animation is complete
        if (isLastItem && destroyWhenEmpty)
        {
            if (owner == PackageOwner.Player1 && p1PickupSystem != null)
                p1PickupSystem.TryManualDrop();
            else if (owner == PackageOwner.Player2 && p2PickupSystem != null)
                p2PickupSystem.TryManualDrop();

            Destroy(gameObject);
        }
    }

    // Remove or simplify PackageEmpty since we handle everything in ThrowItem/SimulateThrow
    private void PackageEmpty(bool isLastItem = false)
    {
        if (itemAvailability == itemCount && !canThrowItems)
        {
            if (destroyWhenEmpty)
            {
                if (owner == PackageOwner.Player1 && p1PickupSystem != null)
                    p1PickupSystem.TryManualDrop();
                else if (owner == PackageOwner.Player2 && p2PickupSystem != null)
                    p2PickupSystem.TryManualDrop();

                Destroy(gameObject);
            }
            else
            {
                originalSprite.SetActive(false);
                emptySprite.SetActive(true);
            }
        }
    }

    
}
