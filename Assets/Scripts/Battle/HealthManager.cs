using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public float Health = 20;
    public float currentHealth;

    private float reviveTime;
    public float reviveSpeed = 1;   

    public bool enemy;
    public HealthManager sharedHealthSource;

    public bool canMove;
    public bool isDefeated;

    [Header("References")]
    public CharacterAnimation characterAnimation;
    public GameObject hand;
    public Image HealthBar;

    private PlayerInputManager playerInputManager;
    private P2Input p2Input;
    private P3Input p3Input;

    private CharacterFlip characterFlip;
    private CharacterMovement characterMovement;
    private ItemSystem cookCharacterSystem;

    [Header("Revive Settings")]
    public KeyCode reviveBoostKey = KeyCode.Space;

    private Rigidbody2D rb2d;
    // Start is called before the first frame update
    void Start()
    {
        playerInputManager = GetComponent<PlayerInputManager>();
        p2Input = GetComponent<P2Input>();
        p3Input = GetComponent<P3Input>();

        characterFlip = GetComponent<CharacterFlip>();
        characterMovement = GetComponent<CharacterMovement>();
        cookCharacterSystem = GetComponent<ItemSystem>();

        currentHealth = Health;
        canMove = true;

        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        bool isPlayer = gameObject.CompareTag("Player");

        if (HealthBar != null) HealthBar.fillAmount = currentHealth / Health;

        if (currentHealth <= 0)
        {
            currentHealth = 0;

            if (isPlayer)
            {
                canMove = false; // on death
                isDefeated = true;

                reviveTime += Time.deltaTime * reviveSpeed;

                if (Input.GetKeyDown(reviveBoostKey))
                {
                    reviveTime += reviveSpeed / 2f;
                }

                if (reviveTime >= Health)
                {
                    currentHealth = Health;

                    canMove = true;
                    isDefeated = false;

                    reviveTime = 0;
                }

                HealthBar.fillAmount = reviveTime / Health;
            }
            else
            {
                if (!isPlayer)
                {
                    GameObject toDestroy = enemy && transform.parent != null ? transform.parent.gameObject : gameObject;
                    Destroy(toDestroy);
                }              
            }
        }

        if (isPlayer)
        {
            SetPlayerActive(canMove,isDefeated);
        }
    }

    public void SetPlayerActive(bool isActive,bool isDefeated)
    {
        if (isActive == false)
        {
            rb2d.velocity = Vector2.zero;
        }

        if (playerInputManager != null) 
        { 
            playerInputManager.enabled = isActive; 
        }
        if (p2Input != null)
        {
            p2Input.enabled = isActive; 
        }
        if (p3Input != null)
        {
            p3Input.enabled = isActive;
        }

        if (characterFlip != null) characterFlip.enabled = isActive;

        if (cookCharacterSystem != null) cookCharacterSystem.canBeCooked = isDefeated;
        if (hand != null) hand.SetActive(!isDefeated);

    }

    public void TryDamage(float damage)
    {
        if (sharedHealthSource != null && sharedHealthSource != this)
        {
            sharedHealthSource.TryDamage(damage);
            return;
        }

        currentHealth -= damage;
    }

}
