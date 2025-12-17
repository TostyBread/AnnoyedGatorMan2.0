using System.Collections;
using UnityEngine;

public class FirearmCrazy : MonoBehaviour
{
    [Header("Crazy Firearm Settings")]
    public float heatThreshold = 100f;
    public float coolRate = 10f;
    public float heatFadeDelay = 2f;
    public float spinForceMin = 300f;
    public float spinForceMax = 800f;
    public GameObject destroyEffectPrefab;
    public float shootIntervalMin = 0.05f;
    public float shootIntervalMax = 0.2f;

    [Header("References")]
    public Collider2D firearmCollider;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private FirearmController firearmController;

    private float currentHeat = 0f;
    private float lastHeatTime = 0f;
    private bool isCrazy = false;
    private SpriteDeformationController deformer;

    void Awake()
    {
        deformer = GetComponent<SpriteDeformationController>();
    }

    void Update()
    {
        if (isCrazy) return;

        // Cool down if not recently heated
        if (Time.time - lastHeatTime > heatFadeDelay && currentHeat > 0f)
        {
            currentHeat = Mathf.Max(0f, currentHeat - coolRate * Time.deltaTime);
            UpdateColor();
        }
    }

    // Call this from DamageSource collision
    public void AddHeat(float heatAmount)
    {
        if (isCrazy) return;

        if (heatAmount > 0f)
        {
            deformer?.TriggerJiggle(1f, 2f, 0.5f, true);
            EffectPool.Instance.SpawnEffect("FoodSteam", transform.position, Quaternion.identity); // Deploy steam anim
        }
        currentHeat += heatAmount;
        lastHeatTime = Time.time;
        UpdateColor();

        if (currentHeat >= heatThreshold)
        {
            StartCoroutine(CrazySequence());
        }
    }

    private void UpdateColor()
    {
        if (spriteRenderer == null) return;
        float t = Mathf.Clamp01(currentHeat / heatThreshold);
        spriteRenderer.color = Color.Lerp(Color.white, Color.red, t);
    }

    private IEnumerator CrazySequence()
    {
        isCrazy = true;
        if (firearmCollider != null)
            firearmCollider.enabled = false;

        int shots = firearmController != null ? firearmController.currentAmmo : 10;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        for (int i = 0; i < shots; i++)
        {
            // Fire in current direction
            firearmController?.Use();

            // Apply random spin force after shooting (except on last shot)
            if (rb != null && i < shots - 1)
            {
                float randomSpinForce = Random.Range(spinForceMin, spinForceMax);
                float spinDirection = Random.Range(-1f, 1f);
                rb.AddTorque(randomSpinForce * spinDirection, ForceMode2D.Impulse);
            }

            // Wait for random interval before next shot
            yield return new WaitForSeconds(Random.Range(shootIntervalMin, shootIntervalMax));
        }

        // Spawn effect and destroy
        if (destroyEffectPrefab != null)
            Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);

        // Remove from any fire source tracking
        foreach (var damageSource in FindObjectsOfType<DamageSource>())
        {
            damageSource.RemoveFromFireObjects(gameObject);
        }

        Destroy(gameObject);
    }
}
