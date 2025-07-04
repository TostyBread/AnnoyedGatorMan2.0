using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightSwitch : MonoBehaviour
{
    [Header("Switch setting")]
    public bool isOn = true;
    public Color lightOffColor;
    private Color originalColor;
    public float requiredImpactForce = 5f;

    public Light2D light2D;
    public string AudioName;

    [Header("Sanity setting")]
    public Sanity sanity;
    public float sanityRecover;
    [Range(0f, 1f)]
    public float sanityRecoverChance = 0.5f;

    [Header("State setting")]
    [Range(0f, 1f)]
    public float stunChance = 0.5f;
    private StateManager stateManager;

    [Header("Sprite References")]
    public GameObject LightOn;
    public GameObject LightOff;

    // Start is called before the first frame update
    void Start()
    {
        originalColor = light2D.color;
    }

    private void Update()
    {
        if (isOn)
        {
            light2D.color = originalColor;
            LightOn.SetActive(true);
            LightOff.SetActive(false);
        }

        if (!isOn)
        {
            light2D.color = lightOffColor;
            LightOff.SetActive(true);
            LightOn.SetActive(false);
        }
    }

    public void ToggleLight(StateManager stateManager)
    {
        if (stateManager != null && stateManager.state == StateManager.PlayerState.Stun) return;

        isOn = !isOn;

        AudioManager.Instance.PlaySound(AudioName, transform.position);

        if (sanity.RemainSanity <= 0)
        {
            if (Random.value <= stunChance && stateManager != null && stateManager.state == StateManager.PlayerState.Idle)
            {
                stateManager.currentStun = 100;
            }
        }

        if (Random.value <= sanityRecoverChance && sanity && sanity.RemainSanity > 0)
        {
            sanity.RemainSanity += sanityRecover;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D rb = collision.rigidbody;
        if (rb != null && collision.relativeVelocity.magnitude >= requiredImpactForce)
        {
            ToggleLight(stateManager);
        }
    }
}
