using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sanity : MonoBehaviour
{
    public float MaxSanity = 10;
    public float RemainSanity;
    public Image SanityBar;

    // Start is called before the first frame update
    void Start()
    {
        RemainSanity = MaxSanity;
    }

    // Update is called once per frame
    void Update()
    {
        SanityBar.fillAmount = RemainSanity / MaxSanity;

        if (RemainSanity < 0)
        {
            RemainSanity = 0;
        }
    }

    public void DecreaseSanity(float decreaseAmount)
    {
        RemainSanity -= decreaseAmount;        
    }
}
