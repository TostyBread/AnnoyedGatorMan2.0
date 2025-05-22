using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public float Health = 20;
    public float currentHealth;
    public float reviveSpeed = 1;
    public Image HealthBar;
    public GameObject hand;
    public bool isPlayer2 = false;

    [Header("References")]
    public CharacterAnimation characterAnimation;

    private PlayerInputManager playerInputManager;
    private CharacterFlip characterFlip;
    private CharacterMovement characterMovement;
    private ItemSystem cookCharacterSystem;
    private HandSpriteManager handSpriteManager;
    private PlayerPickupSystem playerPickupSystem;


    private PlayerInputManagerP2 playerInputManagerP2;
    private CharacterFlipP2 characterFlipP2;
    private HandSpriteManagerP2 handSpriteManagerP2;
    private PlayerPickupSystemP2 playerPickupSystemP2;

    private float reviveTime;
    private bool hasDroppedOnDeath = false;

    void Start()
    {
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
            playerInputManager = GetComponent<PlayerInputManager>();
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
            {
                playerPickupSystemP2?.TryManualDrop();
            }
            else
            {
                playerPickupSystem?.TryManualDrop();
            }
            hasDroppedOnDeath = true;
        }

        if (!CompareTag("Player"))
        {
            Destroy(gameObject);
            return;
        }

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
    }

    private void HandleReviveInput()
    {
        reviveTime += Time.deltaTime * reviveSpeed;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button0))
            reviveTime += reviveSpeed / 2;

        if (reviveTime >= Health)
        {
            currentHealth = Health;
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
    }

    public void TryDamage(float damage)
    {
        currentHealth -= damage;
    }
}
