using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
//Tutorial: https://www.youtube.com/watch?v=DOP_G5bsySA&t=6s

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
    [TextArea(3, 10)]
    public string line;
}

[System.Serializable]
public class Dialogue
{
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
}

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;

    [Header("Next Dialogue Reference")]
    public DialogueManager dialogueManager;
    public GameObject nextDialogue;

    private void OnEnable()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
        if (nextDialogue != null) nextDialogue.SetActive(false);
    }

    public void TriggerDialogue()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue(dialogue);
        else
            Debug.LogWarning("DialogueManager.Instance is null. Did you forget to place it in the scene?");
    }

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasTriggered && collision.CompareTag("Player"))
        {
            TriggerDialogue();
            hasTriggered = true; // prevent repeated triggering

            if (nextDialogue != null) nextDialogue.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
