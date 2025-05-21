using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smoke : MonoBehaviour
{
    [Header("Smoke setting")]
    public GameObject smokePrefeb;
    public float smokeRadius = 2f;
    public float smokeSpreadInterval;
    public int maxSmokeInstances = 10;
    public float smokeMoveSpeed = 1;

    [Header("References")]
    public Window window;
    public LayerMask wallLayer;

    private static int smokeCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpreadSomke());
        smokeCount++;
    }

    // Update is called once per frame
    void Update()
    {
        if (window.isOn)
        {
            transform.position = Vector2.MoveTowards(transform.position, window.transform.position, smokeMoveSpeed * Time.deltaTime);
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
            if (hit == null)
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
}
