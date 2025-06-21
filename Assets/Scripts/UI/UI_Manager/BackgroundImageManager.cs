using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundImageManager : MonoBehaviour
{
    public Image background;

    private Sprite originalImage;
    public bool changeBackground = false;

    // Start is called before the first frame update
    void Start()
    {
        originalImage = background.sprite;
    }

    // Update is called once per frame
    void Update()
    {
        if (changeBackground == false)
        {
            background.sprite = originalImage;
        }
    }

    public void ChangeBackground(Sprite sprite)
    {
        background.sprite = sprite;
        changeBackground = true;
    }
}
