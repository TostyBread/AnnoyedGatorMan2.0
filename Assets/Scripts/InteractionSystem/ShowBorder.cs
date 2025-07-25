using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowBorder : MonoBehaviour
{
    // This script needs to work with DetectTarget script
    public Sprite spriteWithBorder;
    public float spriteWithBorderScale = 1; // Scale factor for the sprite with border

    private ItemSystem itemSystem; // Reference to ItemSystem if needed

    private SpriteRenderer spriteRenderer;
    private Sprite originalSprite;
    private Vector3 originalTransform;
    private Vector3 spriteScale;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalSprite = spriteRenderer.sprite;
        originalTransform = transform.localScale;
        spriteScale = new Vector3(spriteWithBorderScale, spriteWithBorderScale, spriteWithBorderScale); // Set the scale to match the spriteWithBorderScale
        itemSystem = transform.parent.GetComponent<ItemSystem>(); // Get the ItemSystem component if it exists
    }

    public void ShowBorderSprite()
    {
        if (spriteRenderer != null && spriteWithBorder != null)
        {
            spriteRenderer.sprite = spriteWithBorder;
            transform.localScale = spriteScale; // Set the scale to match the spriteScale transform
        }
    }

    public void HideBorderSprite()
    {
        if (spriteRenderer != null && originalSprite != null)
        {
            spriteRenderer.sprite = originalSprite;
            transform.localScale = originalTransform; // Reset the scale to the original transform's scale
        }
    }
}
