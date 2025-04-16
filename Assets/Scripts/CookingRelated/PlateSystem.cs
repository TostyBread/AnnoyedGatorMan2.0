using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlateRequirement
{
    public int itemID;
    public ItemDescriber.CookingState cookingState;
    public ItemDescriber.Condition condition;
    public Vector3 positionOffset;
    public int sortingOrder;
    public bool isFilled = false;
}

public class PlateSystem : MonoBehaviour
{
    private Transform plateParent;
    public List<PlateRequirement> plateRequirements;
    private Dictionary<PlateRequirement, GameObject> placedItems = new Dictionary<PlateRequirement, GameObject>();

    private int customerId = -1;
    public Transform idLabelAnchor;
    public GameObject idLabelPrefab;

    private void Awake()
    {
        plateParent = transform.parent;
        if (plateParent == null)
        {
            Debug.LogError("PlateSystem: No parent object found! Assign a parent plate.");
        }
    }

    public void SetCustomerId(int id)
    {
        customerId = id;
    }

    public int GetCustomerId()
    {
        return customerId;
    }

    public void SpawnLabel(int id)
    {
        if (idLabelPrefab && idLabelAnchor)
        {
            GameObject label = Instantiate(idLabelPrefab, idLabelAnchor.position, Quaternion.identity, idLabelAnchor);
            label.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = id.ToString();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out ItemDescriber itemDescriber)) return;

        PlateRequirement matchedRequirement = GetMatchingRequirement(itemDescriber);
        if (matchedRequirement != null && !matchedRequirement.isFilled)
        {
            AttachToPlate(other.gameObject, matchedRequirement);
            CheckIfAllRequirementsMet();
        }
    }

    private PlateRequirement GetMatchingRequirement(ItemDescriber item)
    {
        foreach (var req in plateRequirements)
        {
            if (!req.isFilled && req.itemID == item.itemID && req.cookingState == item.currentCookingState && req.condition == item.currentCondition)
            {
                return req;
            }
        }
        return null;
    }

    private void AttachToPlate(GameObject item, PlateRequirement requirement)
    {
        item.transform.SetParent(plateParent);
        item.transform.localPosition = requirement.positionOffset;
        placedItems[requirement] = item;
        requirement.isFilled = true;

        UpdateSpriteSorting(item, requirement.sortingOrder);
        FreezeItem(item);
    }

    private void UpdateSpriteSorting(GameObject item, int sortingOrder)
    {
        if (item.TryGetComponent(out SpriteRenderer spriteRenderer))
        {
            spriteRenderer.sortingOrder = sortingOrder;
        }
    }

    private void FreezeItem(GameObject item)
    {
        if (item.TryGetComponent(out Collider2D collider)) collider.enabled = false;

        if (item.TryGetComponent(out Rigidbody2D rb))
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private void CheckIfAllRequirementsMet()
    {
        foreach (var req in plateRequirements)
        {
            if (!req.isFilled)
            {
                AudioManager.Instance.PlaySound("bell1", 1.0f, transform.position);
                return;
            }
        }
        AudioManager.Instance.PlaySound("TaskComplete", 1.0f, transform.position);
    }
}