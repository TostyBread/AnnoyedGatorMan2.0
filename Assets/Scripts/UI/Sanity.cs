using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Sanity : MonoBehaviour
{
    public float MaxSanity = 10;
    public float RemainSanity;
    public Image SanityBar;

    private Dictionary<GameObject, bool> sanityDecreased = new Dictionary<GameObject, bool>();


    // Start is called before the first frame update
    void Start()
    {
        RemainSanity = MaxSanity;
    }

    // Update is called once per frame
    void Update()
    {
        FindFoodObjects();

        if (SanityBar != null) 
        {
            SanityBar.fillAmount = RemainSanity / MaxSanity;
        }
    }

    public void decreaseSanity(float decreaseAmount)
    {
        RemainSanity -= decreaseAmount;
        RemainSanity = Mathf.Clamp(RemainSanity, 0, MaxSanity);
    }

    void FindFoodObjects()
    {
        GameObject[] foodSmall = GameObject.FindGameObjectsWithTag("FoodSmall");
        GameObject[] foodBig =  GameObject.FindGameObjectsWithTag("FoodBig");

        IEnumerable<GameObject> foodObjects = foodSmall.Concat(foodBig);

        //Check if food is overcooked, decrease sanity if yes
        foreach (GameObject food in foodObjects)
        {
            ItemDescriber describer = food.GetComponent<ItemDescriber>();
            if (describer != null && describer.currentCookingState == ItemDescriber.CookingState.Overcooked)
            {
                if (!sanityDecreased.ContainsKey(food))
                {
                    decreaseSanity(10);
                    sanityDecreased[food] = true;
                }
            }
        }
    }
}
