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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (window.weather != WeatherManager.Weather.Rainy) return;

        if (transform.parent != null)
        {
            previousParent = transform.parent;  
        }

        if (transform.parent == null && previousParent != null)
        {
            StartCoroutine(ThunderStrike(spawnDelay));
            previousParent = null;
        }
    }

    IEnumerator ThunderStrike(float sec)
    {
        yield return new WaitForSeconds(sec);
        Debug.Log("Thunder strike");
    }
}
