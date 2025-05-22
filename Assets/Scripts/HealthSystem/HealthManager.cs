using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public float Health = 20;
    public float currentHealth;
    public float reviveSpeed = 1;
    public Image HealthBar;
    public GameObject hand;

    [Header("References")]
    public CharacterAnimation characterAnimation;

    private PlayerInputManager playerInputManager;
    private CharacterFlip characterFlip;
    private CharacterMovement characterMovement;
    private ItemSystem cookCharacterSystem;
    private HandSpriteManager handSpriteManager;
    private PlayerPickupSystem playerPickupSystem;

    private float reviveTime;
    private bool hasDroppedOnDeath = false;

    void Start()
    {
        playerInputManager = GetComponent<PlayerInputManager>();
        characterFlip = GetComponent<CharacterFlip>();
        characterMovement = GetComponent<CharacterMovement>();
        cookCharacterSystem = GetComponent<ItemSystem>();
        playerPickupSystem = GetComponent<PlayerPickupSystem>();
        if (hand != null) handSpriteManager = hand.GetComponent<HandSpriteManager>();

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
            playerPickupSystem?.TryManualDrop();
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

        if (Input.GetKeyDown(KeyCode.Space))
            reviveTime += reviveSpeed / 2;

        if (reviveTime >= Health)
        {
            currentHealth = Health;
            reviveTime = 0;
        }
    }

    private void DisablePlayerControls()
    {
        if (playerInputManager != null) playerInputManager.isInputEnabled = false; 
        if (characterFlip != null) characterFlip.isFlippingEnabled = false;
        characterMovement?.SetMovement(Vector2.zero);

        if (cookCharacterSystem != null)
            cookCharacterSystem.canBeCooked = true;

        if (hand != null) hand.SetActive(false);
        handSpriteManager?.UpdateHandSprite();
    }

    private void EnablePlayerControls()
    {
        if (playerInputManager != null) playerInputManager.isInputEnabled = true; 
        if (characterFlip != null) characterFlip.isFlippingEnabled = true;

        if (cookCharacterSystem != null)
            cookCharacterSystem.canBeCooked = false;

        if (hand != null) hand.SetActive(true);
    }

    public void TryDamage(float damage)
    {
        currentHealth -= damage;
    }
}
