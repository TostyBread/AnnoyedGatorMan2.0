using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
//Tutorial: https://www.youtube.com/watch?v=DOP_G5bsySA&t=6s
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    public Image characterIcon;
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI dialogueArea;

    private Queue<DialogueLine> lines;

    public bool isDialogueActive = false;

    public float typingSpeed = 0.2f;

    public Animator animator;

    private GameObject GameObjectDialogue;

    // Start is called before the first frame update
    void Start()
    {
        GameObjectDialogue = this.gameObject;

        if (Instance == null)
            Instance = this;

        lines = new Queue<DialogueLine>();
    }

    public void StartDialogue(Dialogue dialogue)
    {
        if (dialogue == null || dialogue.dialogueLines.Count == 0)
        {
            Debug.LogWarning("Dialogue is empty or null!");
            return;
        }

        if (lines == null)
            lines = new Queue<DialogueLine>();

        isDialogueActive = true;

        if (animator != null)
            animator.Play("DialogueShow");

        lines.Clear();

        foreach (DialogueLine dialogueLine in dialogue.dialogueLines)
        {
            lines.Enqueue(dialogueLine);
        }

        StartCoroutine(WaitThenDisplayNext(0.5f));
    }

    public void DisplayNextDialogueLine()
    {
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine currentLine = lines.Dequeue();

        characterIcon.sprite = currentLine.character.icon;
        characterName.text = currentLine.character.name; 

        StopAllCoroutines();

        StartCoroutine(TypeSentence(currentLine));

    }

    private IEnumerator TypeSentence(DialogueLine dialogueLine)
    {
        dialogueArea.text = "";
        foreach (char letter in dialogueLine.line.ToCharArray())
        { 
            dialogueArea.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private void EndDialogue() 
    {
        isDialogueActive = false;

        if (animator != null)
            animator.Play("DialogueHide");
    }

    private IEnumerator WaitThenDisplayNext(float delay)
    {
        yield return new WaitForSeconds(delay);
        DisplayNextDialogueLine();
    }
}
