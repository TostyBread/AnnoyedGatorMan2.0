using System.Collections.Generic;
using System.Linq;
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
    private DialogueManager dialogueManager;
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
        CheckItemOnStove
    }

    private CookingStove stove;
    private GameObject player;
    private GameObject wieldingHand;
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

        if (triggerCondition == DialogueConditionType.OnItemPickup)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            wieldingHand = player.transform.Find("HandControls/Wielding_Hand")?.gameObject;
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

    private void CheckItemPickupCondition()
    {
        if (nextTriggered || player == null) return;

        if (wieldingHand == null) return;

        foreach (Transform child in wieldingHand.transform)
        {
            if (itemToTriggerNext != null)
            {
                if (child.name.Contains(itemToTriggerNext.name))
                {
                    nextTriggered = true;
                    TriggerNextDialogue();
                    itemToTriggerNext = null;
                    break;
                }
            }

            Debug.Log("Holding item: " + child);
        }
    }

    private void CheckItemCookedCondition()
    {
        foreach (var item in GameObject.FindGameObjectsWithTag("FoodSmall"))
        {
            if (itemToTriggerNext != null && item.name.Contains(itemToTriggerNext.name))
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
                }
            }
        }
    }

    private void CheckItemOnStove()
    {
        if (stove == null) 
            stove = FindAnyObjectByType<CookingStove>();

        if (itemToTriggerNext != null && stove != null)
        {
            foreach (var item in stove.itemsOnStove)
            {
                if (item.name.Contains(itemToTriggerNext.name))
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
    }

    private void TriggerNextDialogue()
    {
        if (nextDialogue != null)
            nextDialogue.SetActive(true);

        gameObject.SetActive(false);
    }
}
