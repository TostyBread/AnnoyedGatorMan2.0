using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectTarget : MonoBehaviour
{
    public List<string> Tags = new List<string>();
    public List<GameObject> AllItemInRange = new List<GameObject>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        foreach (var tag in Tags)
        {
            if (collision.CompareTag(tag))
            {
                AllItemInRange.Add(collision.gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        foreach (var tag in Tags)
        {
            if (collision.CompareTag(tag))
            {
                AllItemInRange.Remove(collision.gameObject);
            }
        }
    }
}
