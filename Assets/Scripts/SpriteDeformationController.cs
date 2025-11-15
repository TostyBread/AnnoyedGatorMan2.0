using UnityEngine;
using System.Collections.Generic;

public class SpriteDeformationController : MonoBehaviour
{
    private Material materialInstance;

    private float deformationTimer = 0f;
    private float jiggleIntensity = 0f;
    private float squashAmount = 0f;
    private float stretchAmount = 0f;
    private bool isAnimating = false;

    // Coroutine management
    private Coroutine currentDeformationCoroutine;
    private Queue<DeformationRequest> deformationQueue = new Queue<DeformationRequest>();
    private bool isProcessingQueue = false;

    // List of sprite renderers to monitor
    [SerializeField] private List<SpriteRenderer> targetSpriteRenderers = new List<SpriteRenderer>();

    private struct DeformationRequest
    {
        public float jiggle;
        public float squash;
        public float stretch;
        public float speed;
        public float duration;
        public bool shouldOverride;
    }

    private void Start()
    {
        // Auto-populate if empty
        if (targetSpriteRenderers.Count == 0)
        {
            targetSpriteRenderers.AddRange(GetComponentsInChildren<SpriteRenderer>());
        }
    }

    private void Update()
    {
        if (isAnimating && (jiggleIntensity > 0 || squashAmount > 0 || stretchAmount > 0))
        {
            deformationTimer += Time.deltaTime;
            if (materialInstance != null)
            {
                materialInstance.SetFloat("_DeformationTime", deformationTimer);
            }
        }
    }

    private void OnDisable()
    {
        ResetDeformation();
    }

    private void OnEnable()
    {
        if (isAnimating)
        {
            deformationTimer = 0f;
        }
    }

    /// Get the currently active sprite renderer from the list
    private SpriteRenderer GetActiveSpriteRenderer()
    {
        foreach (var spriteRenderer in targetSpriteRenderers)
        {
            if (spriteRenderer != null && spriteRenderer.gameObject.activeInHierarchy)
            {
                return spriteRenderer;
            }
        }

        return null;
    }

    /// Update the material reference from the active sprite renderer
    private void UpdateMaterialReference()
    {
        SpriteRenderer activeSpriteRenderer = GetActiveSpriteRenderer();
        
        if (activeSpriteRenderer != null)
        {
            materialInstance = activeSpriteRenderer.material;
        }
        else
        {
            materialInstance = null;
        }
    }

    /// Trigger a jiggle effect
    public void TriggerJiggle(float intensity, float speed = 5f, float duration = 0.5f, bool shouldOverride = false)
    {
        QueueDeformation(intensity, 0f, 0f, speed, duration, shouldOverride);
    }

    /// Trigger a squash effect
    public void TriggerSquash(float amount, float speed = 5f, float duration = 0.3f, bool shouldOverride = false)
    {
        QueueDeformation(0f, amount, 0f, speed, duration, shouldOverride);
    }

    /// Trigger a stretch effect
    public void TriggerStretch(float amount, float speed = 5f, float duration = 0.3f, bool shouldOverride = false)
    {
        QueueDeformation(0f, 0f, amount, speed, duration, shouldOverride);
    }

    /// Trigger combined deformation effects
    public void TriggerDeformation(float jiggle, float squash, float stretch, float speed = 5f, float duration = 0.5f, bool shouldOverride = false)
    {
        QueueDeformation(jiggle, squash, stretch, speed, duration, shouldOverride);
    }

    /// Queue a deformation request. If shouldOverride is true, interrupts current animation immediately.
    private void QueueDeformation(float jiggle, float squash, float stretch, float speed, float duration, bool shouldOverride = false)
    {
        var request = new DeformationRequest
        {
            jiggle = jiggle,
            squash = squash,
            stretch = stretch,
            speed = speed,
            duration = duration,
            shouldOverride = shouldOverride
        };

        if (shouldOverride && isAnimating)
        {
            // Immediately stop current animation
            if (currentDeformationCoroutine != null)
            {
                StopCoroutine(currentDeformationCoroutine);
                currentDeformationCoroutine = null;
            }

            // Clear the queue completely
            deformationQueue.Clear();
            isProcessingQueue = false;

            // Reset deformation state before starting new one
            ResetDeformation();

            // Enqueue and start the new animation immediately
            deformationQueue.Enqueue(request);
            ProcessDeformationQueue();
        }
        else if (shouldOverride && !isAnimating)
        {
            // Not animating, just clear queue and start new one
            deformationQueue.Clear();
            deformationQueue.Enqueue(request);
            ProcessDeformationQueue();
        }
        else
        {
            // Regular queue behavior (no override)
            deformationQueue.Enqueue(request);
            if (!isProcessingQueue)
            {
                ProcessDeformationQueue();
            }
        }
    }

    private void ProcessDeformationQueue()
    {
        if (deformationQueue.Count == 0)
        {
            isProcessingQueue = false;
            return;
        }

        isProcessingQueue = true;

        // Stop any existing coroutine
        if (currentDeformationCoroutine != null)
        {
            StopCoroutine(currentDeformationCoroutine);
            currentDeformationCoroutine = null;
        }

        var request = deformationQueue.Dequeue();
        
        // Start the new coroutine immediately
        currentDeformationCoroutine = StartCoroutine(PlayDeformation(request.jiggle, request.squash, request.stretch, request.speed, request.duration));
    }

    private System.Collections.IEnumerator PlayDeformation(float jiggle, float squash, float stretch, float speed, float duration)
    {
        isAnimating = true;
        float elapsedTime = 0f;
        deformationTimer = 0f;

        while (elapsedTime < duration)
        {
            // Update material reference each frame to ensure we're using the active sprite renderer
            UpdateMaterialReference();

            float progress = elapsedTime / duration;

            float fadeMultiplier = elapsedTime < duration * 0.5f 
                ? Mathf.Lerp(0f, 1f, progress * 2f)
                : Mathf.Lerp(1f, 0f, (progress - 0.5f) * 2f);

            jiggleIntensity = jiggle * fadeMultiplier;
            squashAmount = squash * fadeMultiplier;
            stretchAmount = stretch * fadeMultiplier;

            if (materialInstance != null)
            {
                materialInstance.SetFloat("_JiggleIntensity", jiggleIntensity);
                materialInstance.SetFloat("_JiggleSpeed", speed);
                materialInstance.SetFloat("_SquashAmount", squashAmount);
                materialInstance.SetFloat("_StretchAmount", stretchAmount);
            }

            deformationTimer += Time.deltaTime;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        ResetDeformation();
        isAnimating = false;
        currentDeformationCoroutine = null;

        // Process next queued animation if any
        ProcessDeformationQueue();
    }

    /// Stop all deformations immediately and clear the queue
    public void StopDeformation()
    {
        if (currentDeformationCoroutine != null)
        {
            StopCoroutine(currentDeformationCoroutine);
            currentDeformationCoroutine = null;
        }

        deformationQueue.Clear();
        ResetDeformation();
        isAnimating = false;
        isProcessingQueue = false;
    }

    /// Internal helper to reset all material properties
    private void ResetDeformation()
    {
        if (materialInstance != null)
        {
            materialInstance.SetFloat("_JiggleIntensity", 0f);
            materialInstance.SetFloat("_SquashAmount", 0f);
            materialInstance.SetFloat("_StretchAmount", 0f);
        }

        jiggleIntensity = 0f;
        squashAmount = 0f;
        stretchAmount = 0f;
        deformationTimer = 0f;
    }
}
