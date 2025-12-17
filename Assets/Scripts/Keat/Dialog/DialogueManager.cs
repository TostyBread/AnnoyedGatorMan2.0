using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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
    private P3Input p3Input;
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
            p3Input = player.GetComponent<P3Input>();

            characterMovement = player.GetComponent<CharacterMovement>();
        }

    }

    void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space) || Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            DisplayNextDialogueLine();
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        PlayerCanMove(false);

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

    private void PlayerCanMove(bool canMove)
    {
        if (canMove)
        {
            //Re-enable player movement and input
            if (playerInputManager != null) 
                playerInputManager.isInputEnabled = true;

            if (p3Input != null)
                p3Input.isInputEnabled = true;
        }
        else if (!canMove)
        {
            //Stop player movement and input
            if (playerInputManager != null)
                playerInputManager.isInputEnabled = false;

            if (p3Input != null)
                p3Input.isInputEnabled = false;

            characterMovement.SetMovement(Vector2.zero);
        }
    }

    public void DisplayNextDialogueLine()
    {
        if (lines.Count == 0)
        {
            if (stayAtLastDialogue == false)
            {
                EndDialogue();
                return;
            }
            else if (stayAtLastDialogue == true) //this code is to stay at the last dialogue line
            {
                PlayerCanMove(true);
                return;
            }
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
        if (playerInputManager != null)
            playerInputManager.isInputEnabled = true;

        if (p3Input != null)
            p3Input.isInputEnabled = true;
    }

    private IEnumerator WaitThenDisplayNext(float delay)
    {
        yield return new WaitForSeconds(delay);
        DisplayNextDialogueLine();
    }

}
