using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClickerDetector : MonoBehaviour
{
    public GameObject[] screens;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider == null)
            {
                foreach (GameObject screen in screens) 
                {
                    screen.SetActive(false);
                }
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            foreach (GameObject screen in screens)
            {
                screen.SetActive(false);
            }
        }
    }
}
