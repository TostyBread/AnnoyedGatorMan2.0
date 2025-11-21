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
    public Collider2D playerCollider; // Player's large ass collider
    public bool isPlayer2 = false;
    public bool isNotPlayer = false; // Condition to check whether its a fire instead of player (Chee Seng tolong pls dont ignore ah)
    public bool isFood = false; // Condition to check whether its a food instead of player

    [Header("Hurt Animation Setting")]
    public bool isHurt = false; // Used for animation to check if player is hurt
    public float isHurtDur = 1; // Duration for hurt animation

    private float lastHealth;
    private float currentHurtDur = 0; // Current duration for hurt animation

    [Header("Shared / Enemy Settings")]
    public bool enemy;
    public HealthManager sharedHealthSource;
    public GameObject deadBody;
    private bool spawnOnceDeadBody = true;

    [Header("Player State Flags")]
    public bool isPlayer = true; // Used to determine if this is a player or not
    public bool canMove = true;
    public bool isDefeated = false;

    [Header("Dialogue Reference")]
    private DialogueManager dialogueManager;

    [Header("References")]
    private PlayerInputManager playerInputManager; 

    private CharacterFlip characterFlip;
    private CharacterMovement characterMovement;
    private ItemSystem cookCharacterSystem;
    private HandSpriteManager handSpriteManager;
    private PlayerPickupSystem playerPickupSystem;

    private PlayerInputManagerP2 playerInputManagerP2;
    private P2Input p2Input;
    private P3Input p3Input;
    private CharacterFlipP2 characterFlipP2;
    private HandSpriteManagerP2 handSpriteManagerP2;
    private PlayerPickupSystemP2 playerPickupSystemP2;
    private P2PickupSystem p2PickupSystem;

    private Rigidbody2D rb2d;

    private float reviveTime;
    private bool hasDroppedOnDeath = false;
    private bool? lastPlayerActiveState = null; // Use this to record the last state player is in. DO NOT UPDATE REGULARLY
    private Sanity sanity;

    private EnemySpawner enemySpawner;
    private SpriteDeformationController deformer; // deformer reference

    [Header("Revive Settings")]
    public KeyCode reviveBoostKey = KeyCode.Space;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        playerInputManager = GetComponent<PlayerInputManager>();

        if (isPlayer2)
        {
            playerInputManagerP2 = GetComponent<PlayerInputManagerP2>();
            p2Input = GetComponent<P2Input>();
            p3Input = GetComponent<P3Input>();
            characterFlipP2 = GetComponent<CharacterFlipP2>();
            handSpriteManagerP2 = GetComponent<HandSpriteManagerP2>();
            playerPickupSystemP2 = GetComponent<PlayerPickupSystemP2>();
            p2PickupSystem = GetComponent<P2PickupSystem>();
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

        // Search for deformer
        deformer = GetComponent<SpriteDeformationController>();

        if (deformer == null)
        {
            deformer = GetComponentInChildren<SpriteDeformationController>();
        }

        cookCharacterSystem = GetComponent<ItemSystem>();
        enemySpawner = FindAnyObjectByType<EnemySpawner>();
        sanity = FindAnyObjectByType<Sanity>();

        currentHealth = Health;
        lastHealth = currentHealth;

        dialogueManager = FindAnyObjectByType<DialogueManager>();
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
            {
                playerPickupSystemP2?.TryManualDrop();
                p2PickupSystem?.TryManualDrop();
            }
            else
                playerPickupSystem?.TryManualDrop();

            hasDroppedOnDeath = true;
        }


        if (!CompareTag("Player"))
        {
            GameObject toDestroy = enemy && transform.parent != null ? transform.parent.gameObject : gameObject; // For kitchen enemy

            if (deadBody != null)
            {
                if (spawnOnceDeadBody == true)
                {
                    GameObject Deadbody = Instantiate(deadBody, this.transform.position, this.transform.rotation); //Spawn dead body of current enemy

                    spawnOnceDeadBody = false;
                }
            }

            Destroy(toDestroy, 0.01f);
            return;
        }

        canMove = false;

        if (isPlayer)
        {
            isDefeated = true;
            sanity.RemainSanity = 0; // Reset sanity when player is defeated
        }
            
        DisablePlayerControls();
        HandleReviveInput();
    }

    private void HandleAliveState()
    {
        hasDroppedOnDeath = false;

        if (dialogueManager != null)
        {
            if (CompareTag("Player") && !dialogueManager.isDialogueActive)
            {
                EnablePlayerControls();
            }
        }
        else
        {
            if (CompareTag("Player"))
            {
                EnablePlayerControls();
            }
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
        if (Input.GetKeyDown(reviveBoostKey))
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

            canMove = true;
            isDefeated = false;
        }
    }

    private void DisablePlayerControls()
    {
        if (isPlayer2)
        {
            if (playerInputManagerP2 != null) playerInputManagerP2.isInputEnabled = false;
            if (p2Input != null) p2Input.isInputEnabled = false;
            if (p3Input != null) p3Input.isInputEnabled = false;
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
            if (p2Input != null) p2Input.isInputEnabled = true;
            if (p3Input != null) p3Input.isInputEnabled = true;
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

        //try to lock all posible control
        canMove = isActive;
        isDefeated = defeated;
        CharacterFlip flip = GetComponent<CharacterFlip>(); 
        if (flip != null)
        {
            flip.isFlippingEnabled = isActive;
        }

        if (!isActive) // Does if-else statement to determine to static or dynamic
        {
            rb2d.velocity = Vector2.zero;
            if (defeated)
            {
                rb2d.bodyType = RigidbodyType2D.Static;
                if (playerCollider != null)
                {
                    playerCollider.enabled = false;
                }
            }
                
        }
        else
        {
            rb2d.bodyType = RigidbodyType2D.Dynamic;
            if (playerCollider != null)
            {
                playerCollider.enabled =true;
            }
        }
    }

    public void TryDamage(float damage , GameObject source) //always set source as this.gameObject
    {
        if (isFood && !source.CompareTag("Enemy"))
            return;

        if (sharedHealthSource != null && sharedHealthSource != this)
        {
            sharedHealthSource.TryDamage(damage,source);
            return;
        }

        if (deformer != null)
        {
            deformer.TriggerShake(0.9f, 10f, 0.2f);
        }
        currentHealth -= damage;
        damageReceived = damage;
    }
}