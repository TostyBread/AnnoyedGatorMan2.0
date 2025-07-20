using UnityEngine;

public class AmmoBox : MonoBehaviour
{
    [Header("Refill Settings")]
    public int ammoRefillAmount = 10; // Set to -1 for full refill
    public string refillSoundName;

    [Header("Snapping Settings")]
    public LayerMask firearmLayer;
    public Transform snapPoint;
    public float snapVelocityThreshold = 0.5f;
    public float bumpDistance = 1f;

    private GameObject currentSnappedGun;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((firearmLayer.value & (1 << other.gameObject.layer)) == 0) return;
        if (other.gameObject == currentSnappedGun) return;

        Rigidbody2D rb = other.attachedRigidbody;
        if (rb == null || rb.velocity.magnitude < snapVelocityThreshold) return;

        FirearmController firearm = other.GetComponent<FirearmController>();
        if (firearm == null) return;

        // Bump existing
        if (currentSnappedGun != null)
        {
            Rigidbody2D prevRb = currentSnappedGun.GetComponent<Rigidbody2D>();
            if (prevRb != null)
            {
                Vector2 bumpDir = ((Vector2)currentSnappedGun.transform.position - (Vector2)snapPoint.position).normalized;
                if (bumpDir == Vector2.zero) bumpDir = Vector2.up;
                Vector2 bumpPos = (Vector2)snapPoint.position + bumpDir * bumpDistance;
                prevRb.MovePosition(bumpPos);
            }
        }

        // Snap
        other.transform.position = snapPoint.position;
        other.transform.rotation = snapPoint.rotation; // reset rotation
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        currentSnappedGun = other.gameObject;

        // Refill
        if (ammoRefillAmount < 0)
        {
            firearm.RefillFull();
        }
        else
        {
            firearm.RefillAmmo(ammoRefillAmount);
        }

        if (!string.IsNullOrEmpty(refillSoundName))
        {
            AudioManager.Instance.PlaySound(refillSoundName, transform.position);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == currentSnappedGun)
        {
            currentSnappedGun = null;
        }
    }
}