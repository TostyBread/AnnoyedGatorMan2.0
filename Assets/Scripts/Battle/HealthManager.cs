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

    [Header("References")]
    public CharacterAnimation characterAnimation;
    public GameObject hand;
    public Image HealthBar;

    private PlayerInputManager playerInputManager;
    private CharacterFlip characterFlip;
    private CharacterMovement characterMovement;

    // Start is called before the first frame update
    void Start()
    {
        playerInputManager = GetComponent<PlayerInputManager>();
        characterFlip = GetComponent<CharacterFlip>();
        characterMovement = GetComponent<CharacterMovement>();

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
                if (characterFlip != null) { characterFlip.enabled = false; }
                if (characterMovement != null) { characterMovement.SetMovement(Vector2.zero); }

                if (hand != null) { hand.SetActive(false); }

                reviveTime += Time.deltaTime * reviveSpeed;
                if (reviveTime >= Health)
                {
                    currentHealth = Health;
                    reviveTime = 0;
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    reviveTime += reviveSpeed;
                }

                HealthBar.fillAmount = reviveTime / Health;
            }
            else
            {
                Destroy(gameObject);                
            }
        }

        if (currentHealth > 0)
        {
            if (gameObject.CompareTag("Player"))
            {
                if (playerInputManager != null) { playerInputManager.enabled = true; }
                if (characterFlip != null) { characterFlip.enabled = true; }

                if (hand != null) { hand.SetActive(true); }
            }
        }
    }

    public void TryDamage(float Damage)
    {
        currentHealth -= Damage;
    }
}
