using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public float damage = 10;
    public bool aimForFood;

    private void Start()
    {
        gameObject.SetActive(false);
        aimForFood = GetComponentInParent<EnemyMovement>().aimForFood;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !aimForFood)
        {
            //Debug.Log("attack " + collision.name);

            collision.GetComponentInChildren<HealthManager>().currentHealth -= damage;
        }

        if (aimForFood && (collision.CompareTag("FoodBig") || collision.CompareTag("FoodSmall")))
        {
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

        if (aimForFood && (collision.gameObject.CompareTag("FoodBig") || collision.gameObject.CompareTag("FoodSmall")))
        {
            if (collision.gameObject.GetComponentInChildren<HealthManager>() != null)
                collision.gameObject.GetComponentInChildren<HealthManager>().currentHealth -= damage;
            else
                Debug.LogWarning(collision + "children has no HealthManager");
        }

        gameObject.SetActive(false);
    }
}
