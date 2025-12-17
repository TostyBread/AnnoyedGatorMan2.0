using System.Collections;
using System.Collections.Generic;
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

    [Header("Reference Do Not Touch")]
    public GameObject currentPlayer; //for DialogueTrigger.cs reference (primary character)

    // Cache all input controllers from both Player1 and Player2
    private List<PlayerInputManager> playerInputManagers = new List<PlayerInputManager>();
    private List<PlayerInputManagerP2> playerInputManagersP2 = new List<PlayerInputManagerP2>();
    private List<P2Input> p2InputControllers = new List<P2Input>();
    private List<P3Input> p3InputControllers = new List<P3Input>();
    private List<CharacterMovement> characterMovements = new List<CharacterMovement>();
    
    // Cache pickup systems for all players
    private List<PlayerPickupSystem> playerPickupSystems = new List<PlayerPickupSystem>();
    private List<P2PickupSystem> p2PickupSystems = new List<P2PickupSystem>();
    private List<PlayerPickupSystemP2> playerPickupSystemsP2 = new List<PlayerPickupSystemP2>();

    void Start()
    {
        if (Instance == null)
            Instance = this;

        lines = new Queue<DialogueLine>();

        // Cache all input controllers from the scene once at start
        CacheAllInputControllers();
    }

    void Update()
    {
        if (isDialogueActive && (Input.GetKeyDown(KeyCode.Space) || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)))
        {
            DisplayNextDialogueLine();
        }
    }

    /// Scans the scene once and caches all input controller references from all players.
    private void CacheAllInputControllers()
    {
        // Clear previous caches
        playerInputManagers.Clear();
        playerInputManagersP2.Clear();
        p2InputControllers.Clear();
        p3InputControllers.Clear();
        characterMovements.Clear();
        playerPickupSystems.Clear();
        p2PickupSystems.Clear();
        playerPickupSystemsP2.Clear();

        // Find all player GameObjects
        GameObject[] allPlayers = new GameObject[] 
        { 
            GameObject.Find("Player1"), 
            GameObject.Find("Player2"), 
            GameObject.Find("Player3") 
        };

        // For each player, find all input controllers and pickup systems
        foreach (var playerObj in allPlayers)
        {
            if (playerObj == null) continue;

            // Get all PlayerInputManager components
            PlayerInputManager[] pims = playerObj.GetComponentsInChildren<PlayerInputManager>();
            playerInputManagers.AddRange(pims);

            // Get all PlayerInputManagerP2 components
            PlayerInputManagerP2[] pimP2s = playerObj.GetComponentsInChildren<PlayerInputManagerP2>();
            playerInputManagersP2.AddRange(pimP2s);

            // Get all P2Input components
            P2Input[] p2Inputs = playerObj.GetComponentsInChildren<P2Input>();
            p2InputControllers.AddRange(p2Inputs);

            // Get all P3Input components
            P3Input[] p3Inputs = playerObj.GetComponentsInChildren<P3Input>();
            p3InputControllers.AddRange(p3Inputs);

            // Get all CharacterMovement components
            CharacterMovement[] charMovements = playerObj.GetComponentsInChildren<CharacterMovement>();
            characterMovements.AddRange(charMovements);

            // Get all PlayerPickupSystem components (P1)
            PlayerPickupSystem[] pPickupSys = playerObj.GetComponentsInChildren<PlayerPickupSystem>();
            playerPickupSystems.AddRange(pPickupSys);

            // Get all P2PickupSystem components (P3)
            P2PickupSystem[] p2PickupSys = playerObj.GetComponentsInChildren<P2PickupSystem>();
            p2PickupSystems.AddRange(p2PickupSys);

            // Get all PlayerPickupSystemP2 components (P2 Kenji)
            PlayerPickupSystemP2[] pPickupSysP2 = playerObj.GetComponentsInChildren<PlayerPickupSystemP2>();
            playerPickupSystemsP2.AddRange(pPickupSysP2);
        }

        // Set the current player reference
        RefreshActivePlayer();

        Debug.Log($"DialogueManager: Cached {playerInputManagers.Count} PlayerInputManager(s), " +
                  $"{playerInputManagersP2.Count} PlayerInputManagerP2(s), " +
                  $"{p2InputControllers.Count} P2Input(s), " +
                  $"{p3InputControllers.Count} P3Input(s), " +
                  $"{characterMovements.Count} CharacterMovement(s), " +
                  $"{playerPickupSystems.Count} PlayerPickupSystem(s), " +
                  $"{p2PickupSystems.Count} P2PickupSystem(s), " +
                  $"and {playerPickupSystemsP2.Count} PlayerPickupSystemP2(s)");
    }

    /// Finds the currently active player (Player1, Player2, or Player3) in the scene.
    private void RefreshActivePlayer()
    {
        // Find all active player GameObjects
        GameObject[] players = new GameObject[] 
        { 
            GameObject.Find("Player1"), 
            GameObject.Find("Player2"), 
            GameObject.Find("Player3") 
        };

        // Find the first active player
        player = null;
        foreach (var p in players)
        {
            if (p != null && p.activeInHierarchy)
            {
                player = p;
                break;
            }
        }

        if (player == null)
        {
            Debug.LogWarning("DialogueManager: No active player found in scene!");
            return;
        }

        // Get the primary character (first child with "Player" tag)
        currentPlayer = FindChildWithTag(player, "Player");
    }

    GameObject FindChildWithTag(GameObject parent, string tag)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.CompareTag(tag))
                return child.gameObject;

            // Recursive search (if nested children)
            GameObject result = FindChildWithTag(child.gameObject, tag);
            if (result != null)
                return result;
        }
        return null;
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

        // Disable all player input
        PlayerCanMove(false);

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
        // Enable/disable all cached input controllers at once
        foreach (var pim in playerInputManagers)
        {
            if (pim != null)
                pim.isInputEnabled = canMove;
        }

        foreach (var pimP2 in playerInputManagersP2)
        {
            if (pimP2 != null)
                pimP2.isInputEnabled = canMove;
        }

        foreach (var p2Input in p2InputControllers)
        {
            if (p2Input != null)
                p2Input.isInputEnabled = canMove;
        }

        foreach (var p3Input in p3InputControllers)
        {
            if (p3Input != null)
                p3Input.isInputEnabled = canMove;
        }

        // Stop all movement when disabling
        if (!canMove)
        {
            foreach (var charMovement in characterMovements)
            {
                if (charMovement != null)
                    charMovement.SetMovement(Vector2.zero);
            }
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

        // Re-enable all player input
        PlayerCanMove(true);
    }

    private IEnumerator WaitThenDisplayNext(float delay)
    {
        yield return new WaitForSeconds(delay);
        DisplayNextDialogueLine();
    }

    // Public accessors for DialogueTrigger to get pickup systems
    public List<PlayerPickupSystem> GetPlayerPickupSystems() => playerPickupSystems;
    public List<P2PickupSystem> GetP2PickupSystems() => p2PickupSystems;
    public List<PlayerPickupSystemP2> GetPlayerPickupSystemsP2() => playerPickupSystemsP2;
}
