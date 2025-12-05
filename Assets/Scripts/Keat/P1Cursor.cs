using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P1Cursor : MonoBehaviour
{
    public SpriteRenderer defaultCursor;

    //Detact target reference
    public GameObject targetPlayer;
    public List<GameObject> childInTargetPlayer = new List<GameObject>();
    public GameObject player;
    public Transform playerWieldingHand;

    private PlayerInputManager playerInputManager;
    private DetectTarget detectTarget;

    private LevelLoader levelLoader;
    private ScoreManager scoreManager;
    private bool shouldShowSystemCursor;
    private Animator animator;


    private void Start()
    {
        levelLoader = FindObjectOfType<LevelLoader>();
        scoreManager = FindObjectOfType<ScoreManager>();

        if (defaultCursor != null)
            animator = defaultCursor.gameObject.GetComponent<Animator>();

        targetPlayer = transform.parent.gameObject;
        if (targetPlayer != null)
        {
            foreach (Transform item in targetPlayer.transform.GetComponentsInChildren<Transform>())
            {
                if (item.name.Contains("New_"))
                {
                    player = item.gameObject;
                    break;
                }
            }
        }

        if (player != null)
        {
            detectTarget = player.GetComponentInChildren<DetectTarget>();
            playerInputManager = player.GetComponent<PlayerInputManager>();

            foreach (Transform item in player.transform.GetComponentsInChildren<Transform>())
            {
                if (item.name == "Wielding_Hand")
                {
                    playerWieldingHand = item;
                    break;
                }
            }
        }
    }

    void Update()
    {
        UpdateCursorVisibility();
        UpdateCursorPosition();

        CursorDetactItem();
    }

    private void UpdateCursorVisibility()
    {
        if (scoreManager != null && scoreManager.gameOver)
        {
            shouldShowSystemCursor = true;
        }
        else
        {
            shouldShowSystemCursor = levelLoader.isShowingSettingScreen;
        }

        Cursor.visible = shouldShowSystemCursor;
        defaultCursor.enabled = !shouldShowSystemCursor;
    }

    private void UpdateCursorPosition()
    {
        if (defaultCursor == null) return;

        // Get world position from existing singleton helper
        Vector3 worldPos = ScreenToWorldPointMouse.Instance.GetMouseWorldPosition();
        defaultCursor.transform.position = worldPos;
    }
    private bool CursorDetactItem()
    {
        if (detectTarget == null) return true;
        if (playerInputManager.canThrow == false && playerWieldingHand.childCount > 0) { animator.Play("CannotThrow_Cursor"); return(true); }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] hits = Physics2D.OverlapPointAll(mousePos);

        foreach (var hit in hits)
        {
            foreach (var obj in detectTarget.AllItemInRange)
            {
                if (hit.gameObject == obj)
                {
                    animator.Play("OnItem_ControllerCursor");
                    return false;
                }
            }
        }

        // No hit, reset cursor
        animator.Play("Normal_ControllerCursor");
        return true;
    }
}
