using System.Collections;
using UnityEngine;

public class ThrownKnifeDamageActivator : MonoBehaviour
{
    [Header("References")]
    public Collider2D damageCollider;

    [Header("Timing")]
    public float activationDelay = 0.1f;

    private Coroutine activationRoutine;

    private void Awake()
    {
        if (damageCollider == null)
            damageCollider = GetComponentInChildren<DamageSource>()?.GetComponent<Collider2D>();

        if (damageCollider != null)
            damageCollider.enabled = false;
    }

    public void EnableDamageDelayed()
    {
        CancelPending();
        activationRoutine = StartCoroutine(DelayedEnable());
    }

    public void DisableDamage()
    {
        CancelPending();

        if (damageCollider != null)
            damageCollider.enabled = false;
    }

    private IEnumerator DelayedEnable()
    {
        yield return new WaitForSeconds(activationDelay);

        if (damageCollider != null)
            damageCollider.enabled = true;

        activationRoutine = null;
    }

    private void CancelPending()
    {
        if (activationRoutine != null)
        {
            StopCoroutine(activationRoutine);
            activationRoutine = null;
        }
    }
}