using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    [Header("Fire setting")]
    public float burnRadius = 2f;
    public float spreadInterval = 3f;
    public int maxFireInstances = 10;
    public GameObject smoke;

    [Header("References")]
    public LayerMask wallLayer;
    private AudioManager audioManager; // Reference to AudioManager for sound management

    private AudioSource fireSound;

    private static int fireCount = 0;
    private bool isPlayingSound = false;

    void Awake()
    {
        audioManager = AudioManager.Instance;
        if (AudioManager.Instance != null) audioManager = AudioManager.Instance; // Get the AudioManager instance

        fireSound = GetComponent<AudioSource>();
    }

    void Start()
    {
        fireCount++;

        StartCoroutine(SpreadFire());
    }

    void Update()
    {
        fireSound.volume = audioManager.AdjustedVolume; // Ensure volume is set from AudioManager

        PlayFireSoundOnce();
    }

    private IEnumerator SpreadFire()
    {
        while (fireCount < maxFireInstances)
        {
            yield return new WaitForSeconds(spreadInterval);

            Vector2 randomOffset = Random.insideUnitCircle * burnRadius;
            Vector2 spawnPosition = (Vector2)transform.position + randomOffset;

            Collider2D hit = Physics2D.OverlapCircle(spawnPosition, 1f, wallLayer);
            if (hit == null)
            {
                Instantiate(this.gameObject, spawnPosition, Quaternion.identity);
                SpreadSmokeOnce();
            }
        }
    }

    private void SpreadSmokeOnce()
    {
        if (smoke == null) return;

        Vector2 randomOffset = Random.insideUnitCircle * burnRadius;
        Vector2 spawnPosition = (Vector2)transform.position + randomOffset;

        Instantiate(smoke, spawnPosition, Quaternion.identity);
    }

    private void PlayFireSoundOnce()
    {
        if (!isPlayingSound && fireSound != null)
        {
            fireSound.Play();
            isPlayingSound = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, burnRadius);
    }

    private void OnDestroy()
    {
        fireCount--;
    }
}
