using UnityEngine;

public class ItemDescriber : MonoBehaviour
{
    public enum CookingState { Uncooked, Cooked, Overcooked }
    public enum Condition { Normal, Bashed, Cut }

    public string itemName;
    public CookingState currentCookingState = CookingState.Uncooked;
    public Condition currentCondition = Condition.Normal;

    private ItemSystem itemSystem;

    void Start()
    {
        itemSystem = GetComponent<ItemSystem>();
        itemName = gameObject.name; // Automatically assign GameObject name
    }

    void Update()
    {
        if (itemSystem != null)
        {
            if (itemSystem.isBurned)
                currentCookingState = CookingState.Overcooked;
            else if (itemSystem.isCooked)
                currentCookingState = CookingState.Cooked;
            else
                currentCookingState = CookingState.Uncooked;
        }
    }
}