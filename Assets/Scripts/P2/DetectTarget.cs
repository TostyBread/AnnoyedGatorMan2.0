using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectTarget : MonoBehaviour
{
    public bool EnemyDetected;
    public List<GameObject> AllItemInRange = new List<GameObject>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("WeaponLong") || collision.CompareTag("WeaponShort") || collision.CompareTag("FoodBig") || collision.CompareTag("FoodSmall"))
        {
            EnemyDetected = true;
            AllItemInRange.Add(collision.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("WeaponLong") || collision.CompareTag("WeaponShort") || collision.CompareTag("FoodBig") || collision.CompareTag("FoodSmall"))  
        {
            EnemyDetected = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("WeaponLong") || collision.CompareTag("WeaponShort") || collision.CompareTag("FoodBig") || collision.CompareTag("FoodSmall"))
        {
            EnemyDetected = false;
            AllItemInRange.Remove(collision.gameObject);
        }
    }
}
