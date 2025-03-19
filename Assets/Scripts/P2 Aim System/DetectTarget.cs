using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectTarget : MonoBehaviour
{
    public bool EnemyDetected;
    public List<GameObject> AllEnemyInRange = new List<GameObject>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("WeaponLong") || collision.CompareTag("WeaponShort") || collision.CompareTag("FoodBig") || collision.CompareTag("FoodSmall"))
        {
            EnemyDetected = true;
            AllEnemyInRange.Add(collision.gameObject);
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
            AllEnemyInRange.Remove(collision.gameObject);
        }
    }
}
