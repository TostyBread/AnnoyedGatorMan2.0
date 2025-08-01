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

    public List<GameObject> brokenPartsUncookedBash;
    public List<GameObject> brokenPartsCookedBash;
    public List<GameObject> brokenPartsUncookedCut;
    public List<GameObject> brokenPartsCookedCut;
    public List<Vector3> brokenPartsOffsets;

    private int currentDurability;
    private float currentCookPoints = 0f;
    public bool isCooked = false;
    public bool isBurned = false;
    private SpriteRenderer cookedSpriteRenderer;
    private Color originalCookedColor;

    private Dictionary<GameObject, float> lastDamageTimestamps = new();
    private float cleanupInterval = 5f;
    private float nextCleanupTime = 0f;

    public enum DamageType { Bash, Cut, Shot }

    void Start()
    {
        currentDurability = durabilityUncooked;
        currentCookPoints = 0f;
        isCooked = false;
        isBurned = false;

        if (cookedState != null)
        {
            cookedSpriteRenderer = cookedState.GetComponent<SpriteRenderer>();
            if (cookedSpriteRenderer != null)
            {
                originalCookedColor = cookedSpriteRenderer.color;
            }
        }
    }

    void Update()
    {
        if (!canBeCooked || isBurned) return;

        if (!isCooked && currentCookPoints >= cookThreshold)
        {
            CookItem();
        }
        else if (!isBurned && currentCookPoints >= burnThreshold)
        {
            BurnItem();
        }

        if (canBeCooked && isCooked && !isBurned && cookedSpriteRenderer != null)
        {
            float range = Mathf.Max(0.01f, burnThreshold - cookThreshold);
            float t = Mathf.Clamp01((currentCookPoints - cookThreshold) / range);

            float r = Mathf.Lerp(originalCookedColor.r, 0f, t);
            float g = Mathf.Lerp(originalCookedColor.g, 0f, t);
            float b = Mathf.Lerp(originalCookedColor.b, 0f, t);

            cookedSpriteRenderer.color = new Color(r, g, b, originalCookedColor.a);
        }

        if (Time.time >= nextCleanupTime)
        {
            CleanupStaleTimestamps();
            nextCleanupTime = Time.time + cleanupInterval;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ApplyCollisionEffect(collision.gameObject);
    }

    public void ApplyCollisionEffect(GameObject source)
    {
        float currentTime = Time.time;
        if (lastDamageTimestamps.TryGetValue(source, out float lastTime))
        {
            if (currentTime - lastTime < damageCooldown) return;
        }

        lastDamageTimestamps[source] = currentTime;

        if (!source.TryGetComponent(out DamageSource sourceDamage)) return;

        DamageType damageType = sourceDamage.damageType == DamageType.Shot ?
            (Random.value > 0.5f ? DamageType.Bash : DamageType.Cut) : sourceDamage.damageType;

        currentDurability -= sourceDamage.damageAmount;

        if (canBeCooked && sourceDamage.heatAmount != 0) // if item can be cook and the heat apply is not 0, execute this
        {
            if (sourceDamage.isFireSource || sourceDamage.isStunSource)
            {
                currentCookPoints += sourceDamage.heatAmount;
            }

            EffectPool.Instance.SpawnEffect("FoodSteam", transform.position, Quaternion.identity); // Deploy steam anim

            if (!isCooked && currentCookPoints >= cookThreshold) CookItem();
            else if (!isBurned && currentCookPoints >= burnThreshold) BurnItem();
        }

        if (currentDurability <= 0 && canBreak) BreakItem(damageType);
    }

    private void CleanupStaleTimestamps()
    {
        float currentTime = Time.time;
        List<GameObject> keysToRemove = new();

        foreach (var entry in lastDamageTimestamps)
        {
            if (entry.Key == null || currentTime - entry.Value > damageCooldown + 1f)
            {
                keysToRemove.Add(entry.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            lastDamageTimestamps.Remove(key);
        }
    }

    private void BreakItem(DamageType damageType)
    {
        if (isBurned) return;

        List<GameObject> breakParts = isCooked
            ? (damageType == DamageType.Bash ? brokenPartsCookedBash : brokenPartsCookedCut)
            : (damageType == DamageType.Bash ? brokenPartsUncookedBash : brokenPartsUncookedCut);

        for (int i = 0; i < breakParts.Count; i++)
        {
            GameObject part = breakParts[i];

            if (part != null)
            {
                part.transform.SetParent(null);
                part.SetActive(true);
                part.transform.position = transform.position + (i < brokenPartsOffsets.Count ? brokenPartsOffsets[i] : Vector3.zero);
            }
        }

        AudioManager.Instance.PlaySound("BrokenBlop", transform.position);
        Destroy(gameObject);
    }

    public void CookItem()
    {
        isCooked = true;
        uncookedState.SetActive(false);
        cookedState.SetActive(true);
        currentDurability = durabilityCooked;
        AudioManager.Instance.PlaySound("Fizzle", transform.position);
    }

    public void BurnItem()
    {
        isBurned = true;
        cookedSpriteRenderer.color = new Color(0f, 0f, 0f, originalCookedColor.a);
        AudioManager.Instance.PlaySound("DryFart", transform.position);
    }
}