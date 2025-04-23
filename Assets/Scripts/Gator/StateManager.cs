using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    public enum PlayerState { Idle, Burn, Freeze, Stun }

    public PlayerState state;
    public float stateDur;

    [Header("Burn State")]
    public float MaxHeat = 100;
    public float currentHeat;
    public float BurnDur = 3;

    private Dictionary<DamageSource, float> cooldowns = new();
    private HashSet<DamageSource> activeSources = new();

    [Header("Freeze State")]
    public float MaxCold = 100;
    public float currentCold;
    public float FreezeDur = 3;

    private Dictionary<DamageSource, float> coldCooldowns = new();
    private HashSet<DamageSource> coldSources = new();

    [Header("References")]
    private float idleMoveSpeed;
    private CharacterMovement characterMovement;

    void Start()
    {
        characterMovement = GetComponent<CharacterMovement>();
        idleMoveSpeed = characterMovement.moveSpeed;
        state = PlayerState.Idle;
    }

    void Update()
    {
        if (characterMovement == null) return;

        if (Input.GetKeyDown(KeyCode.Space) && state == PlayerState.Idle)
        {
            int randomState = Random.Range(1, 4);
            if (randomState == 1) StartCoroutine(Burn(BurnDur));
            else if (randomState == 2) StartCoroutine(Freeze(stateDur));
            else StartCoroutine(Stun(stateDur));
        }

        if (currentHeat >= MaxHeat)
        {
            StartCoroutine(Burn(BurnDur));
            currentHeat = 0;
        }

        if (currentCold >= MaxCold)
        {
            StartCoroutine(Freeze(FreezeDur));
            currentCold = 0;
        }
    }

    void FixedUpdate()
    {
        if (state != PlayerState.Idle) return;

        foreach (var source in activeSources)
        {
            if (!cooldowns.ContainsKey(source))
                cooldowns[source] = source.heatCooldown;

            cooldowns[source] -= Time.fixedDeltaTime;

            if (cooldowns[source] <= 0f)
            {
                currentHeat += source.heatAmount;
                cooldowns[source] = source.heatCooldown;
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
                activeSources.Add(damageSource);
                if (!cooldowns.ContainsKey(damageSource))
                    cooldowns[damageSource] = damageSource.heatCooldown;                
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
                activeSources.Remove(damageSource);
                cooldowns.Remove(damageSource);            
            }

            if (damageSource.isColdSource)
            {
                coldSources.Remove(damageSource);
                coldCooldowns.Remove(damageSource);
            }
        }
    }

    IEnumerator Burn(float dur)
    {
        state = PlayerState.Burn;
        float elapsed = 0f;
        float interval = 0.5f;

        while (elapsed < dur)
        {
            Vector2 randomMovement = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            characterMovement.SetMovement(randomMovement);

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        characterMovement.moveSpeed = idleMoveSpeed;
        state = PlayerState.Idle;
    }

    IEnumerator Freeze(float dur)
    {
        state = PlayerState.Freeze;
        characterMovement.moveSpeed = 1;
        yield return new WaitForSeconds(dur);
        characterMovement.moveSpeed = idleMoveSpeed;
        state = PlayerState.Idle;
    }

    IEnumerator Stun(float dur)
    {
        state = PlayerState.Stun;
        characterMovement.moveSpeed = 0;
        yield return new WaitForSeconds(dur);
        characterMovement.moveSpeed = idleMoveSpeed;
        state = PlayerState.Idle;
    }
}