using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    public enum PlayerState { Idle, Burn, Freeze, Stun }

    public PlayerState state;

    [Header("Burn State")]
    public float MaxHeat = 100;
    public float currentHeat;
    public float BurnDur = 3;
    public string BurnAudioName;
    public Color burnedColor;

    private Dictionary<DamageSource, float> burncooldowns = new();
    private HashSet<DamageSource> burnSources = new();

    [Header("Freeze State")]
    public float MaxCold = 100;
    public float currentCold;
    public float FreezeDur = 3;
    public string FreezeAudioName;
    public Color freezedColor;

    private Dictionary<DamageSource, float> coldCooldowns = new();
    private HashSet<DamageSource> coldSources = new();

    [Header("Stun State")]
    public float MaxStun = 100;
    public float currentStun;
    public float StunDur = 3;
    public string StunAudioName;
    public Color stunnedColor;

    private Dictionary<DamageSource, float> stunCooldowns = new();
    private HashSet<DamageSource> stunSources = new();

    [Header("References")]
    public SpriteRenderer sprite;
    private Color originalColor;
    private float idleMoveSpeed;
    private CharacterMovement characterMovement;
    private HealthManager healthManager;

    void Start()
    {
        characterMovement = GetComponent<CharacterMovement>();
        healthManager = GetComponent<HealthManager>();

        idleMoveSpeed = characterMovement.moveSpeed;
        state = PlayerState.Idle;

        sprite = GetComponentInChildren<SpriteRenderer>();
        originalColor = sprite.color;
    }

    void Update()
    {
        if (characterMovement == null || healthManager.currentHealth <= 0) return;

        //if (Input.GetKeyDown(KeyCode.Space) && state == PlayerState.Idle)
        //{
        //    int randomState = Random.Range(1, 4);
        //    if (randomState == 1) StartCoroutine(Burn(BurnDur));
        //    else if (randomState == 2) StartCoroutine(Freeze(FreezeDur));
        //    else StartCoroutine(Stun(StunDur));
        //}

        if (currentHeat >= MaxHeat)
        {
            StartCoroutine(Burn(BurnDur));
            AudioManager.Instance.PlaySound(BurnAudioName, 1.0f, transform.position);

            currentHeat = 0;
            currentCold = 0;
            currentStun = 0;
        }

        if (currentCold >= MaxCold)
        {
            StartCoroutine(Freeze(FreezeDur));
            AudioManager.Instance.PlaySound(FreezeAudioName, 1.0f, transform.position);

            currentHeat = 0;
            currentCold = 0;
            currentStun = 0;
        }

        if (currentStun >= MaxStun)
        {
            StartCoroutine(Stun(StunDur));
            AudioManager.Instance.PlaySound(StunAudioName, 1.0f, transform.position);

            currentHeat = 0;
            currentCold = 0;
            currentStun = 0;
        }
    }

    void FixedUpdate()
    {
        if (state != PlayerState.Idle) return;

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

        foreach (var source in stunSources)
        {
            if (!stunCooldowns.ContainsKey(source))
                stunCooldowns[source] = source.heatCooldown;

            stunCooldowns[source] -= Time.fixedDeltaTime;

            if (stunCooldowns[source] <= 0f)
            {
                currentStun += source.heatAmount;
                stunCooldowns[source] = source.heatCooldown;
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

            if (damageSource.isStunSource)
            {
                stunSources.Add(damageSource);
                if (!stunCooldowns.ContainsKey(damageSource))
                    stunCooldowns[damageSource] = damageSource.heatCooldown;
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

            if (damageSource.isStunSource)
            {
                stunSources.Remove(damageSource);
                stunCooldowns.Remove(damageSource);
                currentStun = 0;
            }
        }
    }

    IEnumerator Burn(float dur)
    {
        state = PlayerState.Burn;
        if (burnedColor != null) sprite.color = burnedColor;
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
        sprite.color = originalColor;
    }

    IEnumerator Freeze(float dur)
    {
        state = PlayerState.Freeze;
        if (freezedColor != null) sprite.color = freezedColor;
        characterMovement.moveSpeed = 1;
        yield return new WaitForSeconds(dur);
        characterMovement.moveSpeed = idleMoveSpeed;
        sprite.color = originalColor;
        state = PlayerState.Idle;
    }

    IEnumerator Stun(float dur)
    {
        state = PlayerState.Stun;
        if (stunnedColor != null) sprite.color = stunnedColor;
        characterMovement.canMove = false;
        characterMovement.SetMovement(Vector2.zero);
        yield return new WaitForSeconds(dur);
        characterMovement.moveSpeed = idleMoveSpeed;
        characterMovement.canMove = true;
        sprite.color = originalColor;
        state = PlayerState.Idle;
    }
}