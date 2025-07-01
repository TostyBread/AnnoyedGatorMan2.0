using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorBlock : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
        collider.size = spriteRenderer.bounds.size;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
