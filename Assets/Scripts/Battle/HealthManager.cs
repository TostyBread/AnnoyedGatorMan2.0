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
                SetPlayerActive(false); // on death

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
                SetPlayerActive(true);
            }
        }
    }

    private void SetPlayerActive(bool isActive)
    {
        if (playerInputManager != null) playerInputManager.enabled = isActive;
        if (p2Input != null) p2Input.enabled = isActive;
        if (p3Input != null) p3Input.enabled = isActive;

        if (characterFlip != null) characterFlip.enabled = isActive;
        if (cookCharacterSystem != null) cookCharacterSystem.canBeCooked = !isActive;
        if (hand != null) hand.SetActive(isActive);
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
