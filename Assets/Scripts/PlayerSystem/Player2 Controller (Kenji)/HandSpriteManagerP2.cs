using UnityEngine;
using System.Collections;

public class HandSpriteManagerP2 : MonoBehaviour
{
    [Header("Hand Sprite GameObjects")]
    public GameObject fistSprite;            // Default sprite when no item is held
    public GameObject oneHandedSprite;       // GameObject for 1-handed items
    public GameObject twoHandedSprite;       // GameObject for 2-handed items
    public GameObject oneHandedFirearmSprite;  // GameObject for 1-handed firearms
    public GameObject twoHandedFirearmSprite;  // GameObject for 2-handed firearms
    public GameObject throwSprite;          // GameObject for the throw animation sprite

    [Header("References")]
    public PlayerPickupSystemP2 playerPickupSystemP2; // Reference to PlayerPickupSystem

    private Coroutine throwSpriteCoroutine; // Tracks the active throw sprite coroutine

    void Start()
    {
        // Initialize the hand sprite to default state
        UpdateHandSprite();
    }

    public void UpdateHandSprite()
    {
        if (playerPickupSystemP2 == null)
        {
            Debug.LogError("PlayerPickupSystem reference is missing!");
            return;
        }

        // Check if the player is holding an item
        if (playerPickupSystemP2.HasItemHeld)
        {
            // Get the tag of the held item
            string itemTag = playerPickupSystemP2.HeldItemTag;

            // Toggle the appropriate sprite based on the item's tag
            ToggleSprite(itemTag);
        }
        else
        {
            // Default to fist sprite when no item is held
            ToggleSprite("Fist");
        }
    }

    public void ShowThrowSprite(float duration)
    {
        // Cancel any ongoing throw sprite coroutine
        if (throwSpriteCoroutine != null)
        {
            StopCoroutine(throwSpriteCoroutine);
        }

        // Start a new coroutine to briefly show the throw sprite
        throwSpriteCoroutine = StartCoroutine(ActivateThrowSprite(duration));
    }

    private IEnumerator ActivateThrowSprite(float duration)
    {
        // Deactivate all sprites first
        DeactivateAllSprites();

        // Activate the throw sprite
        if (throwSprite != null)
        {
            throwSprite.SetActive(true);
        }

        // Wait for the specified duration
        yield return new WaitForSeconds(duration);

        // After the throw sprite duration, switch back to the fist sprite
        if (fistSprite != null)
        {
            fistSprite.SetActive(true);
        }

        // Ensure the throw sprite is deactivated
        if (throwSprite != null)
        {
            throwSprite.SetActive(false);
        }

        // Clear the coroutine reference
        throwSpriteCoroutine = null;
    }

    private void ToggleSprite(string itemTag)
    {
        // Deactivate all sprites first
        DeactivateAllSprites();

        // Activate the correct sprite based on the tag
        switch (itemTag)
        {
            case "FoodSmall":
                oneHandedSprite.SetActive(true);
                break;
            case "FoodBig":
                twoHandedSprite.SetActive(true);
                break;
            case "WeaponShort":
                oneHandedFirearmSprite.SetActive(true);
                break;
            case "WeaponLong":
                twoHandedFirearmSprite.SetActive(true);
                break;
            case "Fist":
            default:
                if (fistSprite != null)
                {
                    fistSprite.SetActive(true); // Enable the fist GameObject
                }
                break;
        }
    }

    private void DeactivateAllSprites()
    {
        // Deactivate all GameObjects
        if (fistSprite != null) fistSprite.SetActive(false);
        if (oneHandedSprite != null) oneHandedSprite.SetActive(false);
        if (twoHandedSprite != null) twoHandedSprite.SetActive(false);
        if (oneHandedFirearmSprite != null) oneHandedFirearmSprite.SetActive(false);
        if (twoHandedFirearmSprite != null) twoHandedFirearmSprite.SetActive(false);
        if (throwSprite != null) throwSprite.SetActive(false);
    }

    public bool IsFistActive()
    {
        // Return true if the fist GameObject is currently active
        return fistSprite != null && fistSprite.activeSelf;
    }
}