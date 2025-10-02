using System.Collections;
using UnityEngine;

public class CookingStove : MonoBehaviour
{
    public GameObject fireCollider; // Fire collider to apply heat
    public bool isOn = false;
    public float fireActivationDelay = 0.5f; // Delay before fire starts
    public float requiredImpactForce = 5f; // Minimum force required to toggle stove
    public string AudioName;

    public LayerMask itemLayer; // Detect if its the layer to snap on top
    public Transform snapPoint; // Snaps the item to the point
    public float snapVelocityThreshold = 0.5f; // if its in certain velocity, then snap
    public float bumpDistance = 1f; // bumps the existing item's speed

    private GameObject currentSnappedObject;

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
        //Debug.Log("Stove " + (isOn ? "Turning On..." : "Turning Off"));
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
        //Debug.Log("Fire Deactivated");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D rb = collision.rigidbody;
        if (rb != null && rb.velocity.magnitude >= requiredImpactForce)
        {
            ToggleStove();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((itemLayer.value & (1 << other.gameObject.layer)) == 0) return;
        if (other.gameObject == currentSnappedObject) return;

        Rigidbody2D rb = other.attachedRigidbody;
        if (rb == null) return;

        if (rb.velocity.magnitude >= snapVelocityThreshold)
        {
            // Bump previous object
            if (currentSnappedObject != null)
            {
                Rigidbody2D prevRb = currentSnappedObject.GetComponent<Rigidbody2D>();
                if (prevRb != null)
                {
                    Vector2 bumpDir = ((Vector2)currentSnappedObject.transform.position - (Vector2)snapPoint.position).normalized;
                    if (bumpDir == Vector2.zero) bumpDir = Vector2.up; // safety fallback
                    Vector2 bumpPos = (Vector2)snapPoint.position + bumpDir * bumpDistance;
                    prevRb.MovePosition(bumpPos);
                }
            }

            // Snap new object
            other.transform.position = snapPoint.position;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;

            currentSnappedObject = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == currentSnappedObject)
        {
            currentSnappedObject = null;
        }
    }
}