// File: FireExtinguisherProjectile.cs

using UnityEngine;

public class FireExtinguisherProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float lifetime = 5f;
    public float growthDuration = 1.5f;
    public float startScaleValue = 0.2f;
    public float fadeDuration = 1f;

    private float elapsedTime = 0f;
    private float growthElapsedTime = 0f;
    private float fadeElapsedTime = 0f;

    private WeatherManager window;
    public Collider2D FreezeBox;
    private SpriteRenderer spriteRenderer;

    private Vector3 startScale;
    private Vector3 targetScale = Vector3.one;

    private bool isFading = false;
    private bool fadingTriggered = false;

    void Awake()
    {
        window = WeatherManager.Instance;
        FreezeBox.enabled = (window.weather == WeatherManager.Weather.Cold);

        startScale = Vector3.one * startScaleValue;
        transform.localScale = startScale;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        FadingEffect();
        elapsedTime += Time.deltaTime;
        SmokeEnlarge();

        if (elapsedTime >= lifetime)
        {
            TriggerFade();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isFading) return;

        FreezeBox.enabled = false;
        if (TryGetComponent<Collider2D>(out var col))
            col.enabled = false;

        TriggerFade();
    }

    private void DestroyBullet()
    {
        Destroy(gameObject);
    }

    private void TriggerFade()
    {
        if (!fadingTriggered)
        {
            isFading = true;
            fadeElapsedTime = 0f;
            fadingTriggered = true;
        }
    }

    private void FadingEffect()
    {
        if (isFading)
        {
            fadeElapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(fadeElapsedTime / fadeDuration);
            Color currentColor = spriteRenderer.color;
            currentColor.a = Mathf.Lerp(1f, 0f, t);
            spriteRenderer.color = currentColor;

            if (fadeElapsedTime >= fadeDuration)
            {
                DestroyBullet();
            }
            return;
        }
    }

    private void SmokeEnlarge()
    {
        if (growthElapsedTime < growthDuration)
        {
            growthElapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(growthElapsedTime / growthDuration);
            t = Mathf.SmoothStep(0f, 1f, t);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
        }
    }
}