using System.Collections.Generic;
using UnityEngine;

public class ItemSystem : MonoBehaviour
{
    public bool canBeCooked;
    public bool canBreak;
    public int durabilityUncooked;
    public int durabilityCooked;
    public float cookThreshold;
    public float burnThreshold;
    public float damageCooldown = 0.5f;

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
        if (Time.time - lastDamageTime < damageCooldown) return;

        ApplyCollisionEffect(collision.gameObject);
        lastDamageTime = Time.time;
    }

    public void ApplyCollisionEffect(GameObject source)
    {
        if (!source.TryGetComponent(out DamageSource sourceDamage)) return;

        DamageType damageType = sourceDamage.damageType == DamageType.Shot ?
            (Random.value > 0.5f ? DamageType.Bash : DamageType.Cut) : sourceDamage.damageType;

        currentDurability -= sourceDamage.damageAmount;

        if (canBeCooked)
        {
            currentCookPoints += sourceDamage.heatAmount;

            if (!isCooked && currentCookPoints >= cookThreshold) CookItem();
            else if (!isBurned && currentCookPoints >= burnThreshold) BurnItem();
        }

        if (currentDurability <= 0 && canBreak) BreakItem(damageType);
    }

    private void BreakItem(DamageType damageType)
    {
        if (isBurned) return; // Prevent breaking if overcooked

        List<GameObject> breakParts = isCooked
            ? (damageType == DamageType.Bash ? brokenPartsCookedBash : brokenPartsCookedCut)
            : (damageType == DamageType.Bash ? brokenPartsUncookedBash : brokenPartsUncookedCut);

        for (int i = 0; i < breakParts.Count; i++)
        {
            GameObject part = breakParts[i];
            part.transform.SetParent(null);
            part.SetActive(true);
            part.transform.position = transform.position + (i < brokenPartsOffsets.Count ? brokenPartsOffsets[i] : Vector3.zero);
        }

        AudioManager.Instance.PlaySound("BrokenBlop", 1.0f, transform.position);
        Destroy(gameObject);
    }

    public void CookItem()
    {
        isCooked = true;
        uncookedState.SetActive(false);
        cookedState.SetActive(true);
        currentDurability = durabilityCooked;
        AudioManager.Instance.PlaySound("Fizzle", 1.0f, transform.position);
    }

    public void BurnItem()
    {
        isBurned = true;
        cookedState.SetActive(false);
        burnedState.SetActive(true);
        AudioManager.Instance.PlaySound("DryFart", 1.0f, transform.position);
    }
}