using UnityEngine;
using System.Collections.Generic;

public class ItemStatusSystem : MonoBehaviour
{
    [Header("Original Object Reference")]
    public GameObject originalObject; // Assignable reference to the original item sprite/model

    [Header("Break Settings (Before Cooking)")]
    public bool isBreakableBeforeCook = false; // Determines if the item can break before cooking
    public int breakHP = 100;
    public GameObject brokenVersion; // Broken sprite before cooking
    public bool destroyOnBreak = false;
    public GameObject[] spawnOnBreak; // Items to spawn when breaking before cooking
    public bool disableOriginalOnBreak = false;

    [Header("Cook Settings")]
    public bool isCookable = false;
    public int cookHP = 0;
    public int maxCookHP = 100;
    public int burnThreshold = 150;
    public GameObject cookedVersion;
    public GameObject burnedVersion;
    public bool disableOriginalOnCook = false;

    [Header("Break Settings (After Cooking)")]
    public bool isBreakableAfterCook = false; // Determines if the item can break after cooking
    public int breakAfterCookHP = 100;
    public GameObject brokenCookedVersion; // Object to enable when a cooked item breaks
    public GameObject brokenBurnedVersion; // Object to enable when an overcooked item breaks
    public GameObject[] spawnOnCookedBreak; // Items to spawn if the cooked item breaks
    public GameObject[] spawnOnBurnedBreak; // Items to spawn if the overcooked item breaks
    public bool disableCookedOnBreak = false; // Disable cooked item on break
    public bool disableBurnedOnBreak = false; // Disable burned item on break

    [Header("Damage Modifiers (Editable in Inspector)")]
    public List<DamageSource> breakDamageSources = new List<DamageSource>(); // List of break damage sources
    public List<DamageSource> cookDamageSources = new List<DamageSource>(); // List of cooking heat sources

    private bool isOnHeatSource = false;
    private bool isCooked = false;
    private bool isBurned = false;
    private bool isBroken = false;

    void Update()
    {
        if (isCookable && isOnHeatSource)
        {
            ApplyHeat(1); // Apply cooking progress while on heat
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check for break damage sources
        foreach (DamageSource source in breakDamageSources)
        {
            if (collision.gameObject.CompareTag(source.tagName))
            {
                TakeDamage(source.damageAmount);
                return;
            }
        }

        // Check for cook damage sources
        foreach (DamageSource source in cookDamageSources)
        {
            if (collision.gameObject.CompareTag(source.tagName))
            {
                ApplyHeat(source.damageAmount);
                return;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        // If the item was cooked or overcooked, use after-cook HP
        if (isBreakableAfterCook && (isCooked || isBurned))
        {
            breakAfterCookHP -= damage;
            if (breakAfterCookHP <= 0)
            {
                HandleBreakAfterCook();
            }
        }
        else if (isBreakableBeforeCook) // Handle normal break before cooking
        {
            breakHP -= damage;
            if (breakHP <= 0)
            {
                HandleBreakBeforeCook();
            }
        }
    }

    private void HandleBreakBeforeCook()
    {
        if (brokenVersion != null)
        {
            brokenVersion.SetActive(true);
            isBroken = true;
            SpawnItems(spawnOnBreak);
        }
        else if (destroyOnBreak)
        {
            SpawnItems(spawnOnBreak);
            Destroy(gameObject);
            return;
        }

        if (disableOriginalOnBreak && originalObject != null)
        {
            originalObject.SetActive(false);
        }
    }

    private void HandleBreakAfterCook()
    {
        if (isCooked && brokenCookedVersion != null)
        {
            brokenCookedVersion.SetActive(true);
            SpawnItems(spawnOnCookedBreak);
            if (disableCookedOnBreak) cookedVersion.SetActive(false);
        }
        else if (isBurned && brokenBurnedVersion != null)
        {
            brokenBurnedVersion.SetActive(true);
            SpawnItems(spawnOnBurnedBreak);
            if (disableBurnedOnBreak) burnedVersion.SetActive(false);
        }
    }

    public void ApplyHeat(int heatAmount)
    {
        if (!isCookable) return;

        cookHP += heatAmount;

        if (cookHP >= maxCookHP && cookHP < burnThreshold && !isCooked)
        {
            if (cookedVersion != null)
            {
                cookedVersion.SetActive(true);
                isCooked = true;
                DisablePreviousStates(); // NEW: Disable break version if it exists
            }
        }
        else if (cookHP >= burnThreshold && !isBurned)
        {
            if (burnedVersion != null)
            {
                burnedVersion.SetActive(true);
                isBurned = true;
                DisablePreviousStates(); // NEW: Disable cook and break versions
            }
        }
    }

    private void DisablePreviousStates()
    {
        // Disable broken version if it exists
        if (isBroken && brokenVersion != null)
        {
            brokenVersion.SetActive(false);
            isBroken = false;
        }

        // Disable cooked version if overcooked
        if (isCooked && isBurned && cookedVersion != null)
        {
            cookedVersion.SetActive(false);
            isCooked = false;
        }
    }

    private void DisableOriginalIfRequired()
    {
        if (disableOriginalOnCook && originalObject != null)
        {
            originalObject.SetActive(false);
        }
    }

    private void SpawnItems(GameObject[] items)
    {
        foreach (GameObject item in items)
        {
            Instantiate(item, transform.position, Quaternion.identity);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        foreach (DamageSource source in cookDamageSources)
        {
            if (other.CompareTag(source.tagName))
            {
                isOnHeatSource = true;
                return;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        foreach (DamageSource source in cookDamageSources)
        {
            if (other.CompareTag(source.tagName))
            {
                isOnHeatSource = false;
                return;
            }
        }
    }
}

[System.Serializable]
public class DamageSource
{
    public string tagName; // The tag name of the damaging object
    public int damageAmount; // The damage value applied when hit
}