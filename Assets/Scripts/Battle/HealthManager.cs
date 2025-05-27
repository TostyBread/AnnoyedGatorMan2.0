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
    }

    // Update is called once per frame
    void Update()
    {
        if (HealthBar != null) HealthBar.fillAmount = currentHealth / Health;

        if (currentHealth <= 0)
        {
            currentHealth = 0;

            if (gameObject.CompareTag("Player"))
            {
                if (playerInputManager != null) { playerInputManager.enabled = false; }
                if (p2Input != null) { p2Input.enabled = false; }
                if (p3Input != null) { p3Input.enabled = false; }

                if (characterFlip != null) { characterFlip.enabled = false; }
                if (characterMovement != null) { characterMovement.SetMovement(Vector2.zero); }
                if (cookCharacterSystem != null) { cookCharacterSystem.canBeCooked = true; }

                if (hand != null) { hand.SetActive(false); }

                reviveTime += Time.deltaTime * reviveSpeed;
                if (reviveTime >= Health)
                {
                    currentHealth = Health;
                    reviveTime = 0;
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    reviveTime += reviveSpeed / 2;
                }

                HealthBar.fillAmount = reviveTime / Health;
            }
            else
            {
                if (enemy)
                {
                    Destroy(transform.parent.gameObject);
                }
                else
                Destroy(gameObject);                
            }
        }

        if (currentHealth > 0)
        {
            if (gameObject.CompareTag("Player"))
            {
                if (playerInputManager != null) { playerInputManager.enabled = true; }
                if (p2Input != null) { p2Input.enabled = true; }
                if (p3Input != null) { p3Input.enabled = true; }

                if (characterFlip != null) { characterFlip.enabled = true; }
                if (cookCharacterSystem != null) { cookCharacterSystem.canBeCooked = false; }

                if (hand != null) { hand.SetActive(true); }
            }
        }
    }

    public void TryDamage(float Damage)
    {
        currentHealth -= Damage;
    }
}
