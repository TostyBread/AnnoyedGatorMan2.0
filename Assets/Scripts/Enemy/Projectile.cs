using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public List<string> Tags = new List<string>();
    public float Speed;
    private float time;
    public float duration;
    private Rigidbody2D rb2d;

    private void Start()
    {
        time = 0;
        rb2d = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        //handle bullet movement
        rb2d.velocity = transform.right.normalized * Speed;

        //handle bullet lifetime
        time += Time.deltaTime;
        if (time >= duration)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        foreach (string tag in Tags)
        {
            if (collision.CompareTag(tag))
            {
                Destroy(gameObject);
            }
        }
    }
}
