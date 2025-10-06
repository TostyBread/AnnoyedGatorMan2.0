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
    public DialogueManager dialogueManager;
    public GameObject nextDialogue;
    public GameObject showNextGameObject;

    [Header("Trigger Condition")]
    public DialogueConditionType triggerCondition = DialogueConditionType.None;
    public GameObject playerGetToTriggerNext;

    public enum DialogueConditionType { None, OnStove, OnItemPickup }

    private CookingStove stove;
    private GameObject player;
    private GameObject wieldingHand;
    private bool hasTriggered = false;
    private bool nextTriggered = false;

    private void OnEnable()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();

        if (nextDialogue != null)
            nextDialogue.SetActive(false);

        if (triggerCondition == DialogueConditionType.OnStove)
            stove = FindAnyObjectByType<CookingStove>();

        if (triggerCondition == DialogueConditionType.OnItemPickup)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            wieldingHand = player.transform.Find("HandControls/Wielding_Hand")?.gameObject;
        }
    }

    private void Update()
    {
        switch (triggerCondition)
        {
            case DialogueConditionType.OnStove:
                CheckStoveCondition();
                break;

            case DialogueConditionType.OnItemPickup:
                CheckItemPickupCondition();
                break;
        }

        Debug.Log("Player: " + player);
        Debug.Log("Wielding : " + wieldingHand);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasTriggered && collision.CompareTag("Player"))
        {
            hasTriggered = true;
            TriggerDialogue();

            if (triggerCondition == DialogueConditionType.None)
                TriggerNextDialogue();
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
        if (nextTriggered || player == null || playerGetToTriggerNext == null) return;

        if (wieldingHand == null) return;

        foreach (Transform child in wieldingHand.transform)
        {
            if (child.name.Contains(playerGetToTriggerNext.name))
            {
                nextTriggered = true;
                TriggerNextDialogue();
                playerGetToTriggerNext = null;
                break;
            }
        }
    }

    public void TriggerDialogue()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue(dialogue);
        else
            Debug.LogWarning("DialogueManager.Instance is null. Did you forget to place it in the scene?");
    }

    private void TriggerNextDialogue()
    {
        if (nextDialogue != null)
            nextDialogue.SetActive(true);

        if (showNextGameObject != null)
            showNextGameObject.SetActive(true);

        gameObject.SetActive(false);
    }
}
