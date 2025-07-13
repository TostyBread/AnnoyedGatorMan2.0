using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Window : MonoBehaviour
{
    public bool isOn = false;

    [Header("Sprite References")]
    public GameObject WindowOn;
    public GameObject WindowOff;

    private GameObject[] smokes;
    private Smoke[] smoke;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isOn)
        {
            WindowOn.SetActive(true);
            WindowOff.SetActive(false);
        }
        else
        {
            WindowOn.SetActive(false);
            WindowOff.SetActive(true);
        }
    }

    public void SetWindowState(bool state)
    {
        isOn = state;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent<Smoke>(out Smoke smoke))
        {
            smoke.DestroySmoke();
        }
    }
}
