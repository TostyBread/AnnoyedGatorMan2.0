using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeddyBearSpriteManager : MonoBehaviour
{
    public Sprite teddyBearDamagedSprite;

    private HealthManager health;
    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        health = transform.parent.GetComponent<HealthManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!health || !spriteRenderer) return;

        if (health.currentHealth <= health.Health / 2)
        {
            spriteRenderer.sprite = teddyBearDamagedSprite;
        }
    }
}
