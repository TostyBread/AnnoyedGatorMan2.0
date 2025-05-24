using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Window : MonoBehaviour
{
    public bool isOn = false;

    private GameObject[] smokes;
    private Smoke[] smoke;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetWindowState(bool state)
    {
        isOn = state;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Smoke"))
        {
            Destroy(other.gameObject);
        }
    }
}
