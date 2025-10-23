using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public bool stayAtLastDialogue; //DialogueTrigger.cs will change the boolean

    [Header("Player Reference")]
    public GameObject player;
    private PlayerInputManager playerInputManager;
    private CharacterMovement characterMovement;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
            Instance = this;

        lines = new Queue<DialogueLine>();

        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerInputManager = player.GetComponent<PlayerInputManager>();
            characterMovement = player.GetComponent<CharacterMovement>();
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        //Stop player movement and input
        playerInputManager.isInputEnabled = false;
        characterMovement.SetMovement(Vector2.zero);

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
        if (lines.Count == 0 && stayAtLastDialogue == false)
        {
            EndDialogue();
            return;
        }
        else if (lines.Count == 0 && stayAtLastDialogue == true)
        {
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

        //Re-enable player movement and input
        playerInputManager.isInputEnabled = true;
    }

    private IEnumerator WaitThenDisplayNext(float delay)
    {
        yield return new WaitForSeconds(delay);
        DisplayNextDialogueLine();
    }

}
