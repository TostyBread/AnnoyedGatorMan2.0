using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    public float spinSpeed = 100f;

    [Header("Spin Duration")]
    public float minSpinSecond = 1f;
    public float maxSpinSecond = 3f;

    [Header("Reference")]
    public GameObject Knife;

    private float elapsedTime = 0f;

    private float spinDuration;
    private float currentSpeed;

    private void Start()
    {
        spinDuration = Random.Range(minSpinSecond, maxSpinSecond);
    }

    // Update is called once per frame
    void Update()
    {
        if (elapsedTime < spinDuration)
        {
            elapsedTime += Time.deltaTime;

            currentSpeed = Mathf.Lerp(spinSpeed, 0f, elapsedTime / spinDuration);
            transform.Rotate(0, 0, currentSpeed * Time.deltaTime);
        }

        if (currentSpeed == 0)
        {
            GameObject.Instantiate(Knife, transform.position, transform.rotation);
            Destroy(this.gameObject);
        }
    }
}
