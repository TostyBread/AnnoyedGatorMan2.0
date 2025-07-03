using System.Collections;
using UnityEngine;

public class CookingStove : MonoBehaviour
{
    public GameObject fireCollider; // Fire collider to apply heat
    public bool isOn = false;
    public float fireActivationDelay = 0.5f; // Delay before fire starts
    public float requiredImpactForce = 5f; // Minimum force required to toggle stove
    public string AudioName;

    private Coroutine fireActivationCoroutine;

    private void Start()
    {
        fireCollider.SetActive(false); // Ensure fire is off at start
    }

    public void ToggleStove()
    {
        if (isOn)
        {
            isOn = false;
            StopFire();
        }
        else
        {
            isOn = true;
            if (fireActivationCoroutine == null)
            {
                fireActivationCoroutine = StartCoroutine(StartFireWithDelay());
                AudioManager.Instance.PlaySound(AudioName, transform.position);
            }
        }
        Debug.Log("Stove " + (isOn ? "Turning On..." : "Turning Off"));
    }

    private IEnumerator StartFireWithDelay()
    {
        yield return new WaitForSeconds(fireActivationDelay);
        if (isOn) // Ensure stove wasn't turned off during the delay
        {
            fireCollider.SetActive(true);
            Debug.Log("Fire Activated");
        }
        fireActivationCoroutine = null; // Reset coroutine reference
    }

    private void StopFire()
    {
        if (fireActivationCoroutine != null)
        {
            StopCoroutine(fireActivationCoroutine);
            fireActivationCoroutine = null;
        }
        fireCollider.SetActive(false);
        Debug.Log("Fire Deactivated");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D rb = collision.rigidbody;
        if (rb != null && rb.velocity.magnitude >= requiredImpactForce)
        {
            ToggleStove();
        }
    }
}