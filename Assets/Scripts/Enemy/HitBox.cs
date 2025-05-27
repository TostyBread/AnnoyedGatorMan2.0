using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public float damage = 10;
    private void Start()
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //Debug.Log("attack " + collision.name);

            collision.GetComponentInChildren<HealthManager>().currentHealth -= damage;
        }
            gameObject.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //Debug.Log("attack " + collision.gameObject.name);

            collision.gameObject.GetComponentInChildren<HealthManager>().currentHealth -= damage;
        }
            gameObject.SetActive(false);
    }
}
