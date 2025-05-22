using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeSprite : MonoBehaviour
{
    public Sprite[] sprite;

    private SpriteRenderer spriteRenderer;
    private int randomSpriteIndex;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        randomSpriteIndex = Random.Range(0, sprite.Length);
        spriteRenderer.sprite = sprite[randomSpriteIndex];

        transform.localScale = transform.localScale * Random.Range(0.8f,1.2f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
