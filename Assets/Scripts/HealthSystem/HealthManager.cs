using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public float Health = 20;
    public float currentHealth;
    public float reviveSpeed = 1;
    public float damageReceived; // For the duck explosion
    public Image HealthBar;
    public GameObject hand;
    public bool isPlayer2 = false;

    [Header("Shared / Enemy Settings")]
    public bool enemy;
    public HealthManager sharedHealthSource;

    [Header("Player State Flags")]
    public bool canMove = true;
    public bool isDefeated = false;

    [Header("References")]
    public CharacterAnimation characterAnimation;

    private PlayerInputManager playerInputManager;
    private P2Input p2Input;
    private P3Input p3Input;

    private CharacterFlip characterFlip;
    private CharacterMovement characterMovement;
    private ItemSystem cookCharacterSystem;
    private HandSpriteManager handSpriteManager;
    private PlayerPickupSystem playerPickupSystem;

    private PlayerInputManagerP2 playerInputManagerP2;
    private HandSpriteManagerP2 handSpriteManagerP2;
    private PlayerPickupSystemP2 playerPickupSystemP2;

    private Rigidbody2D rb2d;
    private float reviveTime;
    private bool hasDroppedOnDeath = false;

    [Header("Revive Settings")]
    public KeyCode reviveBoostKey = KeyCode.Space;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();

        playerInputManager = GetComponent<PlayerInputManager>();
        p2Input = GetComponent<P2Input>();
        p3Input = GetComponent<P3Input>();

        if (isPlayer2)
        {
            playerInputManagerP2 = GetComponent<PlayerInputManagerP2>();

            handSpriteManagerP2 = GetComponent<HandSpriteManagerP2>();
            playerPickupSystemP2 = GetComponent<PlayerPickupSystemP2>();
            if (hand != null) handSpriteManagerP2 = hand.GetComponent<HandSpriteManagerP2>();
        }
        else
        {
            characterFlip = GetComponent<CharacterFlip>();
            characterMovement = GetComponent<CharacterMovement>();
            playerPickupSystem = GetComponent<PlayerPickupSystem>();
            if (hand != null) handSpriteManager = hand.GetComponent<HandSpriteManager>();
        }

        cookCharacterSystem = GetComponent<ItemSystem>();
        currentHealth = Health;
    }

    void Update()
    {
        UpdateHealthUI();

        if (currentHealth <= 0)
            HandleDeathState();
        else
            HandleAliveState();

        if (CompareTag("Player"))
        {
            SetPlayerActive(canMove, isDefeated);
        }
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

        if (!hasDroppedOnDeath)
        {
            if (isPlayer2)
                playerPickupSystemP2?.TryManualDrop();
            else
                playerPickupSystem?.TryManualDrop();

            hasDroppedOnDeath = true;
        }

        if (!CompareTag("Player"))
        {
            GameObject toDestroy = enemy && transform.parent != null ? transform.parent.gameObject : gameObject;
            Destroy(toDestroy, 0.1f);
            return;
        }

        canMove = false;
        isDefeated = true;

        reviveTime += Time.deltaTime * reviveSpeed;
        if (Input.GetKeyDown(reviveBoostKey) || Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            reviveTime += reviveSpeed / 2;
        }

        if (reviveTime >= Health)
        {
            currentHealth = Health;
            reviveTime = 0;
            canMove = true;
            isDefeated = false;
        }
    }

    private void HandleAliveState()
    {
        hasDroppedOnDeath = false;

        if (CompareTag("Player"))
            EnablePlayerControls();
    }

    private void EnablePlayerControls()
    {
        if (playerInputManager != null) playerInputManager.isInputEnabled = true;
        if (characterFlip != null) characterFlip.isFlippingEnabled = true;

        if (isPlayer2)
        {         
            if (playerInputManagerP2 != null) playerInputManagerP2.isInputEnabled = true;
        }

        if (cookCharacterSystem != null)
            cookCharacterSystem.canBeCooked = false;

        if (hand != null) hand.SetActive(true);
    }

    public void SetPlayerActive(bool isActive, bool defeated)
    {
        if (!isActive && rb2d != null)
        {
            rb2d.velocity = Vector2.zero;

            if (defeated)
            rb2d.bodyType = RigidbodyType2D.Static;
        }
        else
        {
            rb2d.bodyType = RigidbodyType2D.Dynamic;
        }

        if (playerInputManager != null) playerInputManager.isInputEnabled = isActive;
        if (p2Input != null) p2Input.enabled = isActive;
        if (p3Input != null) p3Input.enabled = isActive;
        if (characterFlip != null) characterFlip.enabled = isActive;

        if (isPlayer2)
        {
            if (playerInputManager.isInputEnabled == false)
            {
                playerInputManager.enabled = false;
            }
            else if (playerInputManager.isInputEnabled == true)
            {
                playerInputManager.enabled = true;
            }
        }

        if (cookCharacterSystem != null) cookCharacterSystem.canBeCooked = defeated;
        if (hand != null) hand.SetActive(!defeated);
    }

    private IEnumerator waitFor(float delay)
    { 
        yield return new WaitForSeconds(delay);
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