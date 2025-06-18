using UnityEngine;
using System.Collections;

public class HandSpriteManager : MonoBehaviour
{
    [Header("Hand Sprite GameObjects")]
    public GameObject fistSprite;
    public GameObject oneHandedSprite;
    public GameObject twoHandedSprite;
    public GameObject oneHandedFirearmSprite;
    public GameObject twoHandedFirearmSprite;
    public GameObject throwSprite;

    [Header("References")]
    public PlayerPickupSystem playerPickupSystem;
    public P2PickSystem p2PickSystem;
    public PlayerThrowManager playerThrowManager;

    [Header("If P1, make sure p2PickSystem is null \nIf P2, make sure playerPickupSystem is null")]
    public bool P1FalseP2True;

    private Coroutine throwSpriteCoroutine;

    void Start()
    {
        UpdateHandSprite();
    }

    public void UpdateHandSprite()
    {
        if (!P1FalseP2True && playerPickupSystem != null)
        {
            if (playerPickupSystem.HasItemHeld)
            {
                ToggleSprite(playerPickupSystem.HeldItemTag);
            }
            else
            {
                ToggleSprite("Fist");
            }
        }
        else if (P1FalseP2True && p2PickSystem != null)
        {
            if (p2PickSystem.HasItemHeld)
            {
                ToggleSprite(p2PickSystem.HeldItemTag);
            }
            else
            {
                ToggleSprite("Fist");
            }
        }
        else
        {
            Debug.LogError("HandSpriteManager: Missing valid reference for item pickup system.");
        }
    }

    public void ShowThrowSprite(float duration)
    {
        if (throwSpriteCoroutine != null)
            StopCoroutine(throwSpriteCoroutine);

        throwSpriteCoroutine = StartCoroutine(ActivateThrowSprite(duration));
    }

    private IEnumerator ActivateThrowSprite(float duration)
    {
        DeactivateAllSprites();

        if (throwSprite != null)
            throwSprite.SetActive(true);

        yield return new WaitForSeconds(duration);

        if (fistSprite != null)
            fistSprite.SetActive(true);

        if (throwSprite != null)
            throwSprite.SetActive(false);

        throwSpriteCoroutine = null;
    }

    private void ToggleSprite(string itemTag)
    {
        DeactivateAllSprites();

        switch (itemTag)
        {
            case "FoodSmall":
                oneHandedSprite?.SetActive(true);
                break;
            case "FoodBig":
                twoHandedSprite?.SetActive(true);
                break;
            case "WeaponShort":
                oneHandedFirearmSprite?.SetActive(true);
                break;
            case "WeaponLong":
                twoHandedFirearmSprite?.SetActive(true);
                break;
            case "Fist":
            default:
                fistSprite?.SetActive(true);
                break;
        }
    }

    private void DeactivateAllSprites()
    {
        fistSprite?.SetActive(false);
        oneHandedSprite?.SetActive(false);
        twoHandedSprite?.SetActive(false);
        oneHandedFirearmSprite?.SetActive(false);
        twoHandedFirearmSprite?.SetActive(false);
        throwSprite?.SetActive(false);
    }

    public bool IsFistActive()
    {
        return fistSprite != null && fistSprite.activeSelf;
    }
}