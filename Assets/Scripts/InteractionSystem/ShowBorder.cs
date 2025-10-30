using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowBorder : MonoBehaviour
{
    public Sprite spriteWithBorder;
    public float spriteWithBorderScale = 1;

    private ItemSystem itemSystem;
    private SpriteRenderer spriteRenderer;
    private Sprite originalSprite;
    private Vector3 originalTransform;
    private Vector3 spriteScale;
    private Jiggle jiggleComponent;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalSprite = spriteRenderer.sprite;
        originalTransform = transform.localScale;
        spriteScale = new Vector3(spriteWithBorderScale, spriteWithBorderScale, spriteWithBorderScale);
        itemSystem = transform.parent.GetComponent<ItemSystem>();
        jiggleComponent = GetComponent<Jiggle>();
    }

    public void ShowBorderSprite()
    {
        if (spriteRenderer != null && spriteWithBorder != null)
        {
            spriteRenderer.sprite = spriteWithBorder;

            // If currently jiggling, apply border scale relative to current scale
            if (jiggleComponent != null && jiggleComponent.isJiggling)
            {
                Vector3 currentScale = transform.localScale;
                float currentJiggleRatio = currentScale.x / originalTransform.x;
                transform.localScale = spriteScale * currentJiggleRatio;
            }
            else
            {
                transform.localScale = spriteScale;
            }
        }
    }

    public void HideBorderSprite()
    {
        if (spriteRenderer != null && originalSprite != null)
        {
            spriteRenderer.sprite = originalSprite;

            // If currently jiggling, restore scale relative to jiggle
            if (jiggleComponent != null && jiggleComponent.isJiggling)
            {
                Vector3 currentScale = transform.localScale;
                float currentJiggleRatio = currentScale.x / spriteScale.x;
                transform.localScale = originalTransform * currentJiggleRatio;
            }
            else
            {
                transform.localScale = originalTransform;
            }
        }
    }
}
