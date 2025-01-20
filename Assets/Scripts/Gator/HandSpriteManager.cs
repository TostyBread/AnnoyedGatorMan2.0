using UnityEngine;

public class HandSpriteManager : MonoBehaviour
{
    [Header("Hand Sprite GameObjects")]
    public GameObject fistSprite;            // Default sprite when no item is held
    public GameObject oneHandedSprite;       // GameObject for 1-handed items
    public GameObject twoHandedSprite;       // GameObject for 2-handed items
    public GameObject oneHandedFirearmSprite;  // GameObject for 1-handed firearms
    public GameObject twoHandedFirearmSprite;  // GameObject for 2-handed firearms

    [Header("References")]
    public PlayerPickupSystem playerPickupSystem; // Reference to PlayerPickupSystem

    void Start()
    {
        // Initialize the hand sprite to default state
        UpdateHandSprite();
    }

    public void UpdateHandSprite()
    {
        if (playerPickupSystem == null)
        {
            Debug.LogError("PlayerPickupSystem reference is missing!");
            return;
        }

        // Check if the player is holding an item
        if (playerPickupSystem.HasItemHeld)
        {
            // Get the tag of the held item
            string itemTag = playerPickupSystem.HeldItemTag;

            // Toggle the appropriate sprite based on the item's tag
            ToggleSprite(itemTag);
        }
        else
        {
            // Default to fist sprite when no item is held
            ToggleSprite("Fist");
        }
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
            case "TwoHandedWeapon":
                twoHandedSprite.SetActive(true);
                break;
            case "WeaponShort":
                oneHandedFirearmSprite.SetActive(true);
                break;
            case "Firearm2H":
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
    }

    public bool IsFistActive()
    {
        // Return true if the fist GameObject is currently active
        return fistSprite != null && fistSprite.activeSelf;
    }
}