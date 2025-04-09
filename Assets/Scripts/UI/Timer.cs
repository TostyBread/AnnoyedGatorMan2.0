using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{

    public float MaxTime = 10;
    public float RemainTime;
    public Image TimerBar;

    // Start is called before the first frame update
    void Start()
    {
        RemainTime = MaxTime;
    }

    // Update is called once per frame
    void Update()
    {
        TimerBar.fillAmount = RemainTime / MaxTime;
        RemainTime -= Time.deltaTime;

        RemainTime = Mathf.Clamp(RemainTime, 0, MaxTime);

        //if (Input.GetKey(KeyCode.Space))
        //{
        //    RemainTime += 1;
        //}
    }
}
