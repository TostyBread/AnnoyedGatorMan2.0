using System.Collections.Generic;
using UnityEngine;

#region Serializable Data Classes
[System.Serializable]
public class DialogueCharacter
{
    public string name;
    public Sprite icon;
}

[System.Serializable]
public class DialogueLine
{
    public DialogueCharacter character;
    [TextArea(3, 10)] public string line;
}

[System.Serializable]
public class Dialogue
{
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
}
#endregion

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Data")]
    public Dialogue dialogue;

    [Header("Next Dialogue Trigger")]
    public GameObject nextDialogue;
    public GameObject[] showGameObjects;
    public GameObject[] hideGameObjects;
    private DialogueManager dialogueManager;
    private ScoreManager scoreManager;
    public bool StayAtLastDialogue;

    [Header("Trigger Next Condition")]
    public DialogueConditionType triggerCondition = DialogueConditionType.OnPlayerCollision;
    public GameObject itemToTriggerNext;

    public enum DialogueConditionType 
    { 
        OnPlayerCollision, 
        OnStove, 
        OnItemPickup, 
        CheckItem, 
        CheckItemCooked, 
        CheckItemGone,
        CheckItemOnStove,
        CheckIsClear
    }

    private CookingStove stove;
    private bool hasTriggered = false;
    private bool nextTriggered = false;

    private SpriteRenderer spriteRenderer;

    public bool autoOffStove = false;
    public bool cleanAllFood = false;

    private void OnEnable()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (nextDialogue != null)
            nextDialogue.SetActive(false);

        if (triggerCondition == DialogueConditionType.OnStove || triggerCondition == DialogueConditionType.CheckItemCooked || triggerCondition == DialogueConditionType.CheckItemOnStove)
        { 
            stove = FindAnyObjectByType<CookingStove>();
        }

        if (triggerCondition == DialogueConditionType.CheckIsClear)
        {
            scoreManager = FindObjectOfType<ScoreManager>();            
        }

        dialogueManager.stayAtLastDialogue = StayAtLastDialogue;
    }

    private void Update()
    {
        dialogueManager.stayAtLastDialogue = StayAtLastDialogue;

        switch (triggerCondition)
        {
            case DialogueConditionType.OnStove:
                CheckStoveCondition();
                break;

            case DialogueConditionType.OnItemPickup:
                CheckItemPickupCondition();
                break;
            case DialogueConditionType.CheckItemCooked:
                CheckItemCookedCondition();
                break;
            case DialogueConditionType.CheckItemOnStove:
                CheckItemOnStove();
                break;
            case DialogueConditionType.CheckIsClear:
                CheckIsClearCondition();
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasTriggered && collision.CompareTag("Player"))
        {
            hasTriggered = true;
            TriggerDialogue();

            if (triggerCondition == DialogueConditionType.OnPlayerCollision)
                TriggerNextDialogue();
        }

        switch (triggerCondition)
        {
            case DialogueConditionType.CheckItem:
                if (itemToTriggerNext != null && collision.gameObject.name.Contains(itemToTriggerNext.name))
                {
                    nextTriggered = true;
                    TriggerNextDialogue();
                    itemToTriggerNext = null;
                }
                break;

            case DialogueConditionType.CheckItemGone:
                if (itemToTriggerNext != null && collision.gameObject.name.Contains(itemToTriggerNext.name))
                {
                    GameObject originalItem = GameObject.Find(itemToTriggerNext.name);

                    if (originalItem == null)
                    {
                        nextTriggered = true;
                        TriggerNextDialogue();
                        itemToTriggerNext = null;
                    }
                }
                break;
        }
    }

    private void CheckStoveCondition()
    {
        if (stove != null && stove.isOn && !nextTriggered)
        {
            nextTriggered = true;
            TriggerNextDialogue();
            stove = null;
        }
    }

    /// Checks if ANY active player is holding the specified item
    /// Supports all three pickup systems: PlayerPickupSystem, P2PickupSystem, and PlayerPickupSystemP2
    private void CheckItemPickupCondition()
    {
        if (nextTriggered || itemToTriggerNext == null)
            return;

        // Check P1 pickup system (PlayerPickupSystem)
        foreach (var pickupSys in dialogueManager.GetPlayerPickupSystems())
        {
            if (pickupSys != null && pickupSys.HasItemHeld)
            {
                GameObject heldItem = pickupSys.GetHeldItem();
                if (heldItem != null && heldItem.name.Contains(itemToTriggerNext.name))
                {
                    nextTriggered = true;
                    TriggerNextDialogue();
                    itemToTriggerNext = null;
                    Debug.Log($"P1 Player holding item: {heldItem.name}");
                    return;
                }
            }
        }

        // Check P2/P3 pickup system (P2PickupSystem)
        foreach (var pickupSys in dialogueManager.GetP2PickupSystems())
        {
            if (pickupSys != null && pickupSys.HasItemHeld)
            {
                GameObject heldItem = pickupSys.GetHeldItem();
                if (heldItem != null && heldItem.name.Contains(itemToTriggerNext.name))
                {
                    nextTriggered = true;
                    TriggerNextDialogue();
                    itemToTriggerNext = null;
                    Debug.Log($"P2/P3 Player holding item: {heldItem.name}");
                    return;
                }
            }
        }

        // Check P2 pickup system (PlayerPickupSystemP2)
        foreach (var pickupSys in dialogueManager.GetPlayerPickupSystemsP2())
        {
            if (pickupSys != null && pickupSys.HasItemHeld)
            {
                GameObject heldItem = pickupSys.GetHeldItem();
                if (heldItem != null && heldItem.name.Contains(itemToTriggerNext.name))
                {
                    nextTriggered = true;
                    TriggerNextDialogue();
                    itemToTriggerNext = null;
                    Debug.Log($"P2 Player holding item: {heldItem.name}");
                    return;
                }
            }
        }
    }

    private void CheckItemCookedCondition()
    {
        if (nextTriggered || itemToTriggerNext == null)
            return;

        foreach (var item in GameObject.FindGameObjectsWithTag("FoodSmall"))
        {
            if (item.name.Contains(itemToTriggerNext.name))
            {
                ItemSystem itemSystem = item.GetComponent<ItemSystem>();
                if (itemSystem != null && itemSystem.isCooked && !itemSystem.isBurned)
                {
                    Debug.Log("An item has been cooked: " + item.name);

                    nextTriggered = true;
                    TriggerNextDialogue();

                    if (autoOffStove && stove != null && stove.isOn)
                    {
                        stove.ToggleStove();
                    }

                    if (cleanAllFood)
                    {
                        foreach (var foodItem in GameObject.FindGameObjectsWithTag("FoodSmall"))
                        {
                            Destroy(foodItem);
                        }
                    }

                    itemToTriggerNext = null;
                    return;
                }
            }
        }
    }

    private void CheckItemOnStove()
    {
        if (stove == null) 
            stove = FindAnyObjectByType<CookingStove>();

        if (itemToTriggerNext != null && stove != null && !nextTriggered)
        {
            foreach (var item in stove.itemsOnStove)
            {
                if (item != null && item.name.Contains(itemToTriggerNext.name))
                {
                    Debug.Log("An item is on the stove: " + item.name);
                    nextTriggered = true;
                    TriggerNextDialogue();
                    itemToTriggerNext = null;
                    break;
                }
            }
        }
    }

    private void CheckIsClearCondition()
    {
        if (scoreManager != null && scoreManager.isCleared && !nextTriggered)
        {
            nextTriggered = true;
            TriggerNextDialogue();
        }
    }

    public void TriggerDialogue()
    {
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        if (DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue(dialogue);
        else
            Debug.LogWarning("DialogueManager.Instance is null. Did you forget to place it in the scene?");

        if (showGameObjects != null)
        {
            foreach (var showGameObject in showGameObjects)
                showGameObject.SetActive(true);
        }

        if (hideGameObjects != null)
        {
            foreach (var hideGameObject in hideGameObjects)
                hideGameObject.SetActive(false);
        }
    }

    private void TriggerNextDialogue()
    {
        if (nextDialogue != null)
            nextDialogue.SetActive(true);

        gameObject.SetActive(false);
    }
}
