using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public float Health = 20;
    public float currentHealth;
    public float reviveSpeed = 1;
    public float damageReceived; // From external damage source like explosion
    public Image HealthBar;
    public GameObject hand;
    public bool isPlayer2 = false;
    public bool isNotPlayer = false; // Condition to check whether its a fire instead of player (Chee Seng tolong pls dont ignore ah)

    [Header("Hurt Animation Setting")]
    public bool isHurt = false; // Used for animation to check if player is hurt
    public float isHurtDur = 1; // Duration for hurt animation

    private float lastHealth;
    private float currentHurtDur = 0; // Current duration for hurt animation

    [Header("Shared / Enemy Settings")]
    public bool enemy;
    public HealthManager sharedHealthSource;

    [Header("Player State Flags")]
    public bool canMove = true;
    public bool isDefeated = false;

    [Header("References")]
    public CharacterAnimation characterAnimation;

    private PlayerInputManager playerInputManager;
    // Commented out due to unused. Please consider cleaning up your code and only reference stuff here. Don't overhaul the structure 
    //private P2Input p2Input;
    //private P3Input p3Input;

    private CharacterFlip characterFlip;
    private CharacterMovement characterMovement;
    private ItemSystem cookCharacterSystem;
    private HandSpriteManager handSpriteManager;
    private PlayerPickupSystem playerPickupSystem;

    private PlayerInputManagerP2 playerInputManagerP2;
    private CharacterFlipP2 characterFlipP2;
    private HandSpriteManagerP2 handSpriteManagerP2;
    private PlayerPickupSystemP2 playerPickupSystemP2;

    private Rigidbody2D rb2d;
    private float reviveTime;
    private bool hasDroppedOnDeath = false;
    private bool? lastPlayerActiveState = null; // Use this to record the last state player is in. DO NOT UPDATE REGULARLY

    [Header("Revive Settings")]
    public KeyCode reviveBoostKey = KeyCode.Space;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();

        playerInputManager = GetComponent<PlayerInputManager>();
        //p2Input = GetComponent<P2Input>();
        //p3Input = GetComponent<P3Input>();

        if (isPlayer2)
        {
            playerInputManagerP2 = GetComponent<PlayerInputManagerP2>();
            characterFlipP2 = GetComponent<CharacterFlipP2>();
            handSpriteManagerP2 = GetComponent<HandSpriteManagerP2>();
            playerPickupSystemP2 = GetComponent<PlayerPickupSystemP2>();
            if (hand != null) handSpriteManagerP2 = hand.GetComponent<HandSpriteManagerP2>();
        }
        else
        {
            playerInputManager = GetComponent<PlayerInputManager> ();
            characterFlip = GetComponent<CharacterFlip>();
            characterMovement = GetComponent<CharacterMovement>();
            playerPickupSystem = GetComponent<PlayerPickupSystem>();
            if (hand != null) handSpriteManager = hand.GetComponent<HandSpriteManager>();
        }

        cookCharacterSystem = GetComponent<ItemSystem>();
        currentHealth = Health;
        lastHealth = currentHealth;
    }

    void Update()
    {
        UpdateHealthUI();

        if (currentHealth <= 0)
            HandleDeathState();
        else
            HandleAliveState();
    }

    private void UpdateHealthUI()
    {
        if (HealthBar == null) return;
        float value = currentHealth <= 0 ? reviveTime : currentHealth;
        HealthBar.fillAmount = value / Health;
    }

    private void HandleDeathState()
    {
        currentHealth = 0;

        if (!hasDroppedOnDeath) // Handle player dropping item when 0 health
        {
            if (isPlayer2)
                playerPickupSystemP2?.TryManualDrop();
            else
                playerPickupSystem?.TryManualDrop();

            hasDroppedOnDeath = true;
        }

        if (!CompareTag("Player"))
        {
            GameObject toDestroy = enemy && transform.parent != null ? transform.parent.gameObject : gameObject; // For kitchen enemy
            Destroy(toDestroy, 0.1f);
            return;
        }

        canMove = false;
        isDefeated = true;

        DisablePlayerControls();
        HandleReviveInput();
    }

    private void HandleAliveState()
    {
        hasDroppedOnDeath = false;

        if (CompareTag("Player"))
        {
            EnablePlayerControls();
        }

        if (lastHealth > currentHealth)
        {
            isHurt = true;
            currentHurtDur = isHurtDur; // Reset hurt duration
            lastHealth = currentHealth;
        }
        else if (isHurt)
        {
            currentHurtDur -= Time.deltaTime; // Decrease hurt duration
            if (currentHurtDur <= 0) isHurt = false; // Reset hurt state when duration ends
        }
    }

    private void HandleReviveInput()
    {
        reviveTime += Time.deltaTime * reviveSpeed;
        if (Input.GetKeyDown(reviveBoostKey) || Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            reviveTime += reviveSpeed / 2;
        }

        if (reviveTime >= Health)
        {
            currentHealth = Health;

            //Reset Animation
            lastHealth = currentHealth;
            isHurt = false;
            currentHurtDur = 0;

            reviveTime = 0;
        }
    }

    private void DisablePlayerControls()
    {
        if (isPlayer2)
        {
            if (playerInputManagerP2 != null) playerInputManagerP2.isInputEnabled = false;
            if (characterFlipP2 != null) characterFlipP2.isFlippingEnabled = false;
            characterMovement?.SetMovement(Vector2.zero);

            if (cookCharacterSystem != null)
                cookCharacterSystem.canBeCooked = true;

            if (hand != null) hand.SetActive(false);
            handSpriteManagerP2?.UpdateHandSprite();
        }
        else
        {
            if (playerInputManager != null) playerInputManager.isInputEnabled = false;
            if (characterFlip != null) characterFlip.isFlippingEnabled = false;
            characterMovement?.SetMovement(Vector2.zero);

            if (cookCharacterSystem != null)
                cookCharacterSystem.canBeCooked = true;

            if (hand != null) hand.SetActive(false);
            handSpriteManager?.UpdateHandSprite();
        }
        SetPlayerActiveOnce(false, true); // Execute player rigidbody to static
    }

    private void EnablePlayerControls()
    {
        if (isPlayer2)
        {
            if (playerInputManagerP2 != null) playerInputManagerP2.isInputEnabled = true;
            if (characterFlipP2 != null) characterFlipP2.isFlippingEnabled = true;
        }
        else
        {
            if (playerInputManager != null) playerInputManager.isInputEnabled = true;
            if (characterFlip != null) characterFlip.isFlippingEnabled = true;
        }

        if (cookCharacterSystem != null)
            cookCharacterSystem.canBeCooked = false;

        if (hand != null) hand.SetActive(true);
        SetPlayerActiveOnce(true, false); // Execute player rigidbody to dynamic
    }

    private void SetPlayerActiveOnce(bool isActive, bool defeated) // Handle player rigidbody 2D state
    {
        if (!rb2d || isNotPlayer) return;

        if (lastPlayerActiveState.HasValue && lastPlayerActiveState.Value == isActive) // Records player last active state
            return;

        lastPlayerActiveState = isActive; // Assign the value

        if (!isActive) // Does if-else statement to determine to static or dynamic
        {
            rb2d.velocity = Vector2.zero;
            if (defeated)
                rb2d.bodyType = RigidbodyType2D.Static;
        }
        else
        {
            rb2d.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    public void TryDamage(float damage)
    {
        if (sharedHealthSource != null && sharedHealthSource != this)
        {
            sharedHealthSource.TryDamage(damage);
            return;
        }

        currentHealth -= damage;
        damageReceived = damage;
    }
}