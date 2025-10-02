using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AmmoBox : MonoBehaviour
{
    [Header("Refill Settings")]
    public int ammoRefillAmount = 10; // Set to -1 for full refill
    public string refillSoundName;
    
    [Header("Timed Refill Settings")]
    public float refillDuration = 10f; // Time in seconds to refill ammo
    //public string refillStartSoundName = "AmmoRefillStart";
    //public string refillCompleteSoundName = "AmmoRefillComplete";
    
    [Header("Progress Bar UI")]
    public GameObject progressBarUI; // Direct reference to progress bar GameObject
    public Image progressBarFill; // Direct reference to the fill Image component

    [Header("Snapping Settings")]
    public LayerMask firearmLayer;
    public Transform snapPoint;
    public float snapVelocityThreshold = 0.5f;
    public float bumpDistance = 1f;
    
    [Header("Overlap Detection")]
    public float overlapCheckInterval = 0.5f; // How often to check for overlaps

    private GameObject currentSnappedGun;
    private Coroutine overlapCheckCoroutine;
    private Coroutine refillCoroutine;
    private Collider2D triggerCollider;
    private bool isRefilling = false;
    private Jiggle jiggle; // Reference to the Jiggle component

    private void Start()
    {
        triggerCollider = GetComponent<Collider2D>();
        jiggle = GetComponent<Jiggle>();

        // Start checking for overlaps periodically
        overlapCheckCoroutine = StartCoroutine(CheckForOverlaps());
        
        // Initialize progress bar as hidden
        if (progressBarUI != null)
        {
            progressBarUI.SetActive(false);
        }
        
        // Auto-find progress bar components if not assigned
        if (progressBarUI == null)
        {
            progressBarUI = GetComponentInChildren<Canvas>()?.gameObject;
        }
        
        if (progressBarFill == null && progressBarUI != null)
        {
            progressBarFill = progressBarUI.GetComponentInChildren<Image>();
        }
    }

    private void OnDestroy()
    {
        if (overlapCheckCoroutine != null)
        {
            StopCoroutine(overlapCheckCoroutine);
        }
        
        if (refillCoroutine != null)
        {
            StopCoroutine(refillCoroutine);
        }
    }

    private IEnumerator CheckForOverlaps()
    {
        while (true)
        {
            yield return new WaitForSeconds(overlapCheckInterval);
            CheckAndProcessOverlappingFirearms();
        }
    }

    private void CheckAndProcessOverlappingFirearms()
    {
        if (triggerCollider == null) return;

        // Get all colliders overlapping with our trigger
        Collider2D[] overlapping = Physics2D.OverlapBoxAll(
            triggerCollider.bounds.center,
            triggerCollider.bounds.size,
            0f,
            firearmLayer
        );

        foreach (var collider in overlapping)
        {
            if (collider.gameObject == currentSnappedGun) continue;
            
            FirearmController firearm = collider.GetComponent<FirearmController>();
            if (firearm == null) continue;

            Rigidbody2D rb = collider.attachedRigidbody;
            if (rb == null) continue;

            // Process the firearm regardless of velocity since it's already overlapping
            ProcessSnapFirearm(collider, rb, firearm, true);
            break; // Only process the first valid firearm found
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((firearmLayer.value & (1 << other.gameObject.layer)) == 0) return;
        if (other.gameObject == currentSnappedGun) return;

        Rigidbody2D rb = other.attachedRigidbody;
        if (rb == null || rb.velocity.magnitude < snapVelocityThreshold) return;

        FirearmController firearm = other.GetComponent<FirearmController>();
        if (firearm == null) return;

        ProcessSnapFirearm(other, rb, firearm, false);
    }

    private void ProcessSnapFirearm(Collider2D other, Rigidbody2D rb, FirearmController firearm, bool forceSnap)
    {
        // Only check velocity if not forcing the snap
        if (!forceSnap && rb.velocity.magnitude < snapVelocityThreshold) return;

        jiggle.StartJiggle(); //jiggle start when starting to refill

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
            
            // Stop any ongoing refill process
            StopRefillProcess();
        }

        // Snap
        other.transform.position = snapPoint.position;
        other.transform.rotation = snapPoint.rotation; // reset rotation
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        currentSnappedGun = other.gameObject;

        // Start timed refill process
        StartRefillProcess(firearm);
    }

    private void StartRefillProcess(FirearmController firearm)
    {
        if (isRefilling) return;
        
        // Play start sound
        //if (!string.IsNullOrEmpty(refillStartSoundName))
        //{
        //    AudioManager.Instance.PlaySound(refillStartSoundName, transform.position);
        //}
        
        // Show and initialize progress bar
        ShowProgressBar();
        
        // Start refill coroutine
        refillCoroutine = StartCoroutine(RefillAmmoOverTime(firearm));
    }

    private void StopRefillProcess()
    {
        if (refillCoroutine != null)
        {
            StopCoroutine(refillCoroutine);
            refillCoroutine = null;
        }
        
        isRefilling = false;
        HideProgressBar();
    }

    private void ShowProgressBar()
    {
        if (progressBarUI != null)
        {
            progressBarUI.SetActive(true);
        }
        
        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = 0f;
        }
    }

    private void HideProgressBar()
    {
        if (progressBarUI != null)
        {
            progressBarUI.SetActive(false);
        }
    }

    private IEnumerator RefillAmmoOverTime(FirearmController firearm)
    {
        isRefilling = true;
        float elapsedTime = 0f;
        
        while (elapsedTime < refillDuration)
        {
            // Update progress bar
            if (progressBarFill != null)
            {
                progressBarFill.fillAmount = elapsedTime / refillDuration;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Complete the refill
        CompleteRefill(firearm);
    }

    private void CompleteRefill(FirearmController firearm)
    {
        // Fill progress bar completely
        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = 1f;
        }
        
        // Refill ammo
        if (ammoRefillAmount < 0)
        {
            firearm.RefillFull();
        }
        else
        {
            firearm.RefillAmmo(ammoRefillAmount);
        }

        // Play completion sounds
        //if (!string.IsNullOrEmpty(refillCompleteSoundName))
        //{
        //    AudioManager.Instance.PlaySound(refillCompleteSoundName, transform.position);
        //}
        
        if (!string.IsNullOrEmpty(refillSoundName))
        {
            AudioManager.Instance.PlaySound(refillSoundName, transform.position);
        }

        jiggle.StartJiggle(); //jiggle start after refill

        // Clean up
        isRefilling = false;
        StartCoroutine(DelayedProgressBarHide());
    }

    private IEnumerator DelayedProgressBarHide()
    {
        yield return new WaitForSeconds(0.5f); // Show completed bar briefly
        HideProgressBar();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == currentSnappedGun)
        {
            currentSnappedGun = null;
            StopRefillProcess();
        }
    }

    // Public method to manually trigger overlap check (useful for external systems)
    public void ForceCheckOverlaps()
    {
        CheckAndProcessOverlappingFirearms();
    }
}