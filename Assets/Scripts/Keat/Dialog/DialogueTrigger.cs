using System.Collections;
using System.Collections.Generic;
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
    [TextArea(3,10)]
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
        }
    }
}
