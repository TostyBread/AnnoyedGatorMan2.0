using System.Collections.Generic;
using UnityEngine;

public class DetectTarget : MonoBehaviour
{
    public List<string> Tags = new List<string>();
    public List<string> Layers = new List<string>();
    public List<GameObject> AllItemInRange = new List<GameObject>();

    private void Update()
    {
        for (int i = AllItemInRange.Count - 1; i >= 0; i--)
        {
            GameObject obj = AllItemInRange[i];

            // First, handle destroyed/null Unity objects safely
            if (obj == null)
            {
                AllItemInRange.RemoveAt(i);
                continue;
            }

            // Now it's safe to access members on the GameObject
            if (obj.CompareTag("Enemy") || obj.CompareTag("Player"))
            {
                // Use TryGetComponent to avoid exceptions and slightly better performance
                if (!obj.TryGetComponent<HealthManager>(out _))
                {
                    AllItemInRange.RemoveAt(i);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        foreach (var tag in Tags)
        {
            if (collision.CompareTag(tag))
            {
                if (!AllItemInRange.Contains(collision.gameObject))
                {
                    AllItemInRange.Add(collision.gameObject);
                }

                ShowBorder showBorder = collision.GetComponentInChildren<ShowBorder>();
                if (showBorder != null)
                {
                    showBorder.ShowBorderSprite();
                }
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

            ShowBorder showBorder = collision.GetComponentInChildren<ShowBorder>();
            if (showBorder != null)
            {
                showBorder.HideBorderSprite();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (var tag in Tags)
        {
            if (collision.gameObject.CompareTag(tag))
            {
                AllItemInRange.Add(collision.gameObject);
            }

            ShowBorder showBorder = collision.gameObject.GetComponentInChildren<ShowBorder>();
            if (showBorder != null)
            {
                showBorder.ShowBorderSprite();
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        foreach (var tag in Tags)
        {
            if (collision.gameObject.CompareTag(tag))
            {
                AllItemInRange.Remove(collision.gameObject);
            }

            ShowBorder showBorder = collision.gameObject.GetComponentInChildren<ShowBorder>();
            if (showBorder != null)
            {
                showBorder.HideBorderSprite();
            }
        }
    }
}
