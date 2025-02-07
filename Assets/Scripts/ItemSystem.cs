using System.Collections.Generic;
using UnityEngine;

// 1. Item System: Handles durability, cooking, breaking, and hitpoints
public class ItemSystem : MonoBehaviour
{
    public bool canBeCooked;
    public bool canBreak;
    public int durabilityUncooked;
    public int durabilityCooked;
    public float cookThreshold;
    public float burnThreshold;
    public float damageCooldown = 0.5f; // Cooldown time in seconds

    public GameObject uncookedState;
    public GameObject cookedState;
    public GameObject burnedState;
    public List<GameObject> brokenPartsUncookedBash;
    public List<GameObject> brokenPartsCookedBash;
    public List<GameObject> brokenPartsUncookedCut;
    public List<GameObject> brokenPartsCookedCut;
    public List<Vector3> brokenPartsOffsets;

    private int currentDurability;
    private float currentCookPoints = 0f;
    public bool isCooked = false;
    public bool isBurned = false;
    private float lastDamageTime = -1f;

    public enum DamageType { Bash, Cut, Shot }

    void Start()
    {
        currentDurability = durabilityUncooked;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (Time.time - lastDamageTime >= damageCooldown)
        {
            ApplyCollisionEffect(collision.gameObject);
            lastDamageTime = Time.time; // Update last damage time
        }
    }

    public void ApplyCollisionEffect(GameObject source)
    {
        DamageSource sourceDamage = source.GetComponent<DamageSource>();
        if (sourceDamage == null) return;

        DamageType damageType = sourceDamage.damageType;
        int damage = sourceDamage.damageAmount;
        float heat = sourceDamage.heatAmount;

        if (damageType == DamageType.Shot)
            damageType = Random.value > 0.5f ? DamageType.Bash : DamageType.Cut;

        currentDurability -= damage;

        if (canBeCooked)
        {
            currentCookPoints += heat;

            if (!isCooked && currentCookPoints >= cookThreshold)
            {
                CookItem();
            }
            else if (!isBurned && currentCookPoints >= burnThreshold)
            {
                BurnItem();
            }
        }

        if (currentDurability <= 0 && canBreak)
        {
            BreakItem(damageType);
        }
    }

    private void BreakItem(DamageType damageType)
    {
        List<GameObject> breakParts;
        if (isCooked)
        {
            breakParts = damageType == DamageType.Bash ? brokenPartsCookedBash : brokenPartsCookedCut;
        }
        else
        {
            breakParts = damageType == DamageType.Bash ? brokenPartsUncookedBash : brokenPartsUncookedCut;
        }

        for (int i = 0; i < breakParts.Count; i++)
        {
            breakParts[i].transform.SetParent(null);
            breakParts[i].SetActive(true);
            if (i < brokenPartsOffsets.Count)
            {
                breakParts[i].transform.position = transform.position + brokenPartsOffsets[i];
            }
            else
            {
                breakParts[i].transform.position = transform.position;
            }
        }
        Destroy(gameObject);
    }

    public void CookItem()
    {
        isCooked = true;
        uncookedState.SetActive(false);
        cookedState.SetActive(true);
        currentDurability = durabilityCooked;
    }

    public void BurnItem()
    {
        isBurned = true;
        cookedState.SetActive(false);
        burnedState.SetActive(true);
    }
}
