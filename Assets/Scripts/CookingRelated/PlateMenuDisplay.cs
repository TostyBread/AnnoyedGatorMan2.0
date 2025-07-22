using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlateMenuDisplay : MonoBehaviour
{
    [System.Serializable]
    public class FoodIcon
    {
        public int itemID;
        public GameObject iconObject;
    }

    public List<FoodIcon> foodIcons;

    private Dictionary<int, List<GameObject>> iconLookup;
    private Dictionary<GameObject, bool> filledIcons; // Track filled status

    private void Awake()
    {
        iconLookup = new Dictionary<int, List<GameObject>>();
        filledIcons = new Dictionary<GameObject, bool>();

        foreach (var food in foodIcons)
        {
            if (!iconLookup.ContainsKey(food.itemID))
                iconLookup[food.itemID] = new List<GameObject>();

            iconLookup[food.itemID].Add(food.iconObject);
            filledIcons[food.iconObject] = false;
        }
    }

    public void MarkAsFilled(int itemID)
    {
        if (!iconLookup.TryGetValue(itemID, out List<GameObject> icons)) return;

        foreach (var icon in icons)
        {
            if (!filledIcons[icon])
            {
                filledIcons[icon] = true;

                if (icon.TryGetComponent(out SpriteRenderer sr))
                {
                    sr.color = new Color(0.2f, 0.2f, 0.2f);
                }
                else if (icon.TryGetComponent(out Image img))
                {
                    img.color = new Color(0.2f, 0.2f, 0.2f);
                }
                else
                {
                    Debug.LogWarning($"Icon with itemID {itemID} has no SpriteRenderer or Image.");
                }

                return; // Only mark one per call
            }
        }
    }
}