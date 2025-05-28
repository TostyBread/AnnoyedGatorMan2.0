using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Smoke : MonoBehaviour
{
    [Header("Smoke setting")]
    public GameObject smokePrefeb;
    public bool willSpread = true;
    public float smokeRadius = 2f;
    public float smokeSpreadInterval;
    public int maxSmokeInstances = 10;
    public float smokeMoveSpeed = 1;

    [Header("Weed Smoke setting")]
    public bool isWeed = false;
    public float sanityRecover = 10;
    public bool isSmoking = false;
    public string SmokeAudioName;

    [Header("Random scale & rotation")]
    public float Min;
    public float Max;

    public float rotationMin;
    public float rotationMax;

    [Header("References")]
    public LayerMask wallLayer;

    private GameObject smokingPlayer;
    private Sanity sanity;
    private Window window;
    private static int smokeCount = 0;


    // Start is called before the first frame update
    void Awake()
    {
        sanity = GameObject.FindGameObjectWithTag("Sanity").GetComponent<Sanity>();
        window = GameObject.FindGameObjectWithTag("Window").GetComponent<Window>();

        float scale = Random.Range(Min, Max);
        transform.localScale = new Vector2(scale, scale);
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(rotationMin, rotationMax));

        if (willSpread)
        {
            StartCoroutine(SpreadSomke());
            smokeCount++;            
        } 
    }

    // Update is called once per frame
    void Update()
    {
        if (window.isOn)
        {
            transform.position = Vector2.MoveTowards(transform.position, window.transform.position, smokeMoveSpeed * Time.deltaTime);
        }

        if (sanity != null && isWeed && sanity.RemainSanity < sanity.MaxSanity) StartSmoking();
    }

    private void StartSmoking()
    {
        if (isWeed && isSmoking)
        {
            // Calculate distance to the player and adjust smoke speed base on distance
            float distance = Vector2.Distance(transform.position, smokingPlayer.transform.position);
            float adjustedSpeed = smokeMoveSpeed * distance;

            // Move toward the player while shriking
            transform.position = Vector2.MoveTowards(transform.position, smokingPlayer.transform.position, adjustedSpeed * Time.deltaTime);
            transform.localScale -= transform.localScale * Time.deltaTime;

            if (transform.localScale.x <= 0.3f)
            {
                sanity.RemainSanity += sanityRecover;
                AudioManager.Instance.PlaySound(SmokeAudioName, 1.0f, transform.position);
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator SpreadSomke()
    {
        while (smokeCount < maxSmokeInstances)
        {
            yield return new WaitForSeconds(smokeSpreadInterval);

            Vector2 randomOffset = Random.insideUnitCircle * smokeRadius;
            Vector2 spawnPosition = (Vector2)transform.position + randomOffset;

            Collider2D hit = Physics2D.OverlapCircle(spawnPosition, 1f, wallLayer);
            if (hit == null && smokePrefeb)
            {
                Instantiate(smokePrefeb, spawnPosition, Quaternion.identity);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, smokeRadius);
    }

    public void SetSmokeState(bool state, GameObject player)
    {
        isSmoking = state;
        smokingPlayer = player;
    }
}
