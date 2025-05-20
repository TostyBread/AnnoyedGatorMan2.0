using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightSwitch : MonoBehaviour
{
    [Header("Switch setting")]
    public bool isOn = true;
    public float requiredImpactForce = 5f;

    public Light2D light2D;
    private float lightIntensity;
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

    // Start is called before the first frame update
    void Start()
    {
        lightIntensity = light2D.intensity;
    }

    public void ToggleLight(StateManager stateManager)
    {
        if (stateManager != null && stateManager.state == StateManager.PlayerState.Stun) return;

        isOn = !isOn;

        AudioManager.Instance.PlaySound(AudioName, 1.0f, transform.position);
        light2D.intensity = isOn? lightIntensity : 0.02f;

        if (Random.value <= stunChance && stateManager != null && stateManager.state == StateManager.PlayerState.Idle)
        {
            stateManager.currentStun = 100;
        }

        Debug.Log("Light " + (isOn ? "Turning On..." : "Turning Off"));

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
