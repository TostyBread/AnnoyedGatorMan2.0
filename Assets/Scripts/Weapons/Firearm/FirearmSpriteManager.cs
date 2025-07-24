using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirearmSpriteManager : MonoBehaviour
{
    //Animator will overwrite the white border sprite, so another method is needed to show the animation
    public Sprite firearmIdleSprite;
    public Sprite firearmFireSprite;

    private SpriteRenderer spriteRenderer;
    private FirearmController firearmController;

    // Start is called before the first frame update
    void Start()
    {
        firearmController = transform.parent.GetComponent<FirearmController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (firearmController == null && spriteRenderer == null) return;

        if (firearmController.owner != null && firearmController.isFiring)
        {
            spriteRenderer.sprite = firearmFireSprite;
        }
        else if (firearmController.owner != null && !firearmController.isFiring)
        {
            spriteRenderer.sprite = firearmIdleSprite;
        }
    }
}
