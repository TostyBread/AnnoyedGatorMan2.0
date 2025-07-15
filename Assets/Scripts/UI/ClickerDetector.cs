using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ClickerDetector : MonoBehaviour
{
    public GameObject[] screens;

    private BackgroundImageManager backgroundImageManager;

    private void Start()
    {
        backgroundImageManager = FindObjectOfType<BackgroundImageManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider == null)
            {
                if (backgroundImageManager != null) backgroundImageManager.changeBackground = false;

                foreach (GameObject screen in screens) 
                {
                    screen.SetActive(false);
                }
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            if (backgroundImageManager != null) backgroundImageManager.changeBackground = false;

            foreach (GameObject screen in screens)
            {
                screen.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePopUpScreens();
        }
    }

    public void ClosePopUpScreens()
    {
        if (backgroundImageManager != null) backgroundImageManager.changeBackground = false;
        foreach (GameObject screen in screens)
        {
            screen.SetActive(false);
        }
    }
}
