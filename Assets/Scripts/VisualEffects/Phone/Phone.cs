using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phone : MonoBehaviour
{
    public GameObject thunder;
    public float spawnDelay;

    [Header("References")]
    public WeatherManager window;

    private Transform previousParent;
    private Coroutine currentCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (window.weather != WeatherManager.Weather.Rainy) return;
        if (currentCoroutine != null) return;

        if (transform.parent != null)
        {
            previousParent = transform.parent;  
        }

        if (transform.parent == null && previousParent != null)
        {
            currentCoroutine = StartCoroutine(Spawnthunder(spawnDelay));
            previousParent = null;
        }
    }

    IEnumerator Spawnthunder(float sec)
    {
        yield return new WaitForSeconds(sec);
        GameObject.Instantiate(thunder, transform.position, Quaternion.identity);

        currentCoroutine = null;
    }
}
