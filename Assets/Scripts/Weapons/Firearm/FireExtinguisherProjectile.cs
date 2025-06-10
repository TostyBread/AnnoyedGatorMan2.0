using UnityEngine;

public class FireExtinguisherProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float lifetime = 5f;
    public float growthDuration = 1.5f;
    public float startScaleValue = 0.2f;

    private float elapsedTime = 0f;
    private float growthElapsedTime = 0f;

    private WeatherManager window;
    public Collider2D FreezeBox;

    private Vector3 startScale;
    private Vector3 targetScale = Vector3.one;

    void Awake()
    {
        window = WeatherManager.Instance;
        FreezeBox.enabled = (window.weather == WeatherManager.Weather.Cold);

        startScale = Vector3.one * startScaleValue;
        transform.localScale = startScale;
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        if (growthElapsedTime < growthDuration)
        {
            growthElapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(growthElapsedTime / growthDuration);
            t = Mathf.SmoothStep(0f, 1f, t);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
        }

        if (elapsedTime >= lifetime)
        {
            DestroyBullet();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        DestroyBullet();
    }

    private void DestroyBullet()
    {
        Destroy(gameObject);
    }
}