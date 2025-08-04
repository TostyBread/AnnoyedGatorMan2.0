using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingScreenChangePage : MonoBehaviour
{
    public GameObject KeyboardAndMousePage;
    public GameObject ControllerPage;

    // Start is called before the first frame update
    void Start()
    {
        KeyboardAndMousePage.SetActive(true);
        ControllerPage.SetActive(false);
    }

    public void ChangeToKeyboardAndMousePage()
    {
        KeyboardAndMousePage.SetActive(true);
        ControllerPage.SetActive(false);
    }

    public void ChangeToControllerPage()
    {
        KeyboardAndMousePage.SetActive(false);
        ControllerPage.SetActive(true);
    }
}
