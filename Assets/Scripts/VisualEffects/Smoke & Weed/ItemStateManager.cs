using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StateManager;

public class ItemStateManager : MonoBehaviour
{
    public enum ItemState { Idle, Burn, Freeze }

    public ItemState state;

    [Header("Burn State")]
    public float MaxHeat = 100;
    public float currentHeat;
    public string BurnAudioName;
    public GameObject spawnObject;

    private Dictionary<DamageSource, float> burncooldowns = new();
    private HashSet<DamageSource> burnSources = new();

    [Header("Freeze State")]
    public float MaxCold = 100;
    public float currentCold;
    public string FreezeAudioName;

    private Dictionary<DamageSource, float> coldCooldowns = new();
    private HashSet<DamageSource> coldSources = new();

    [Header("References")]
    private ItemSystem itemSystem;

    void Start()
    {
        itemSystem = GetComponent<ItemSystem>();
        state = ItemState.Idle;
    }

    void Update()
    {
        if (currentHeat >= MaxHeat)
        {
            ItemBurn();
            AudioManager.Instance.PlaySound(BurnAudioName, 1.0f, transform.position);

            currentHeat = 0;
            currentCold = 0;
        }

        if (currentCold >= MaxCold)
        {
            ItemFreeze();
            AudioManager.Instance.PlaySound(FreezeAudioName, 1.0f, transform.position);

            currentHeat = 0;
            currentCold = 0;
        }
    }

    void FixedUpdate()
    {
        if (state != ItemState.Idle) return;

        foreach (var source in burnSources)
        {
            if (!burncooldowns.ContainsKey(source))
                burncooldowns[source] = source.heatCooldown;

            burncooldowns[source] -= Time.fixedDeltaTime;

            if (burncooldowns[source] <= 0f)
            {
                currentHeat += source.heatAmount;
                burncooldowns[source] = source.heatCooldown;
            }
        }

        foreach (var source in coldSources)
        {
            if (!coldCooldowns.ContainsKey(source))
                coldCooldowns[source] = source.heatCooldown;

            coldCooldowns[source] -= Time.fixedDeltaTime;

            if (coldCooldowns[source] <= 0f)
            {
                currentCold += source.heatAmount;
                coldCooldowns[source] = source.heatCooldown;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out DamageSource damageSource))
        {
            if (damageSource.isFireSource)
            {
                burnSources.Add(damageSource);
                if (!burncooldowns.ContainsKey(damageSource))
                    burncooldowns[damageSource] = damageSource.heatCooldown;
            }

            if (damageSource.isColdSource)
            {
                coldSources.Add(damageSource);
                if (!coldCooldowns.ContainsKey(damageSource))
                    coldCooldowns[damageSource] = damageSource.heatCooldown;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out DamageSource damageSource))
        {
            if (damageSource.isFireSource)
            {
                burnSources.Remove(damageSource);
                burncooldowns.Remove(damageSource);
                currentHeat = 0;
            }

            if (damageSource.isColdSource)
            {
                coldSources.Remove(damageSource);
                coldCooldowns.Remove(damageSource);
                currentCold = 0;
            }
        }
    }

    private void ItemBurn()
    {
        state = ItemState.Burn;
        GameObject.Instantiate(spawnObject, transform.position, Quaternion.identity);
        Destroy(this.gameObject, 0.1f);
    }

    private void ItemFreeze()
    {
        state = ItemState.Freeze;
        if (itemSystem) itemSystem.cookThreshold *= 2;
    }
}
