using System.Collections.Generic;
using UnityEngine;

public class DetectTarget : MonoBehaviour
{
    public List<string> Tags = new List<string>();
    public List<string> Layers = new List<string>();

    public List<GameObject> AllItemInRange = new List<GameObject>();
    private HashSet<GameObject> uniqueItems = new HashSet<GameObject>();

    private void LateUpdate()
    {
        // Sync: HashSet → List
        AllItemInRange.Clear();

        foreach (var obj in uniqueItems)
        {
            if (obj != null)
                AllItemInRange.Add(obj);
        }
    }

    private bool IsValidTag(Collider2D col)
    {
        foreach (var tag in Tags)
            if (col.CompareTag(tag))
                return true;

        return false;
    }

    private void AddObject(GameObject obj)
    {
        if (uniqueItems.Add(obj))   // returns false if already inside
        {
            var border = obj.GetComponentInChildren<ShowBorder>();
            border?.ShowBorderSprite();
        }
    }

    private void RemoveObject(GameObject obj)
    {
        if (uniqueItems.Remove(obj))
        {
            var border = obj.GetComponentInChildren<ShowBorder>();
            border?.HideBorderSprite();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsValidTag(collision))
            AddObject(collision.gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (IsValidTag(collision))
            RemoveObject(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsValidTag(collision.collider))
            AddObject(collision.gameObject);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (IsValidTag(collision.collider))
            RemoveObject(collision.gameObject);
    }

    private void OnDisable()
    {
        AllItemInRange.Clear();
    }
}