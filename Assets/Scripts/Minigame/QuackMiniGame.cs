using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class QuackMiniGame : MonoBehaviour
{
    public float sliderSpeed = 1;

    private Slider slider;
    private bool reachEnd = false;

    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        SliderMovement();
    }

    private void SliderMovement()
    {
        if (!reachEnd)
        {
            slider.value += 0.1f * sliderSpeed * Time.deltaTime;
        }
        else if (reachEnd)
        {
            slider.value -= 0.1f * sliderSpeed * Time.deltaTime;
        }

        if (slider.value == 1 || slider.value == 0)
        {
            reachEnd = !reachEnd;
        }
    }
}
