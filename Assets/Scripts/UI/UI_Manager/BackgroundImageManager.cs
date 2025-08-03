using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundImageManager : MonoBehaviour
{
    public Animator FallingGator;

    // Start is called before the first frame update
    void Start()
    {
        FallingGator.gameObject.SetActive(false);
    }

    public void ShowFallingGator()
    {
        FallingGator.gameObject.SetActive(true);
        FallingGator.Play("FallingGatorAnimation");
    }

    public void HideFallingGator()
    {
        FallingGator.gameObject.SetActive(false);
    }
}
