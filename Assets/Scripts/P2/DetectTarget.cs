using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectTarget : MonoBehaviour
{
    public List<string> Tags = new List<string>();
    public bool EnemyDetected;
    public List<GameObject> AllItemInRange = new List<GameObject>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        foreach (var tags in Tags)
        {
            if (collision.CompareTag(tags))
            {
                EnemyDetected = true;
                AllItemInRange.Add(collision.gameObject);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        foreach (var tags in Tags)
        {
            if (collision.CompareTag(tags))
            {
                EnemyDetected = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        foreach (var tags in Tags)
        {
            if (collision.CompareTag(tags))
            {
                EnemyDetected = false;
                AllItemInRange.Remove(collision.gameObject);
            }
        }
    }
}
