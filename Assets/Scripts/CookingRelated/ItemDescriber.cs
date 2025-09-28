using UnityEngine;

public class ItemDescriber : MonoBehaviour
{
    public enum CookingState { Uncooked, Cooked, Overcooked }
    public enum Condition { Normal, Bashed, Cut }

    public int itemID; // itemID Must be manually assigned
    public CookingState currentCookingState = CookingState.Uncooked;
    public Condition currentCondition = Condition.Normal;
    
    // Add plate attachment tracking
    public bool isAttachedToPlate { get; private set; } = false;
    public PlateSystem attachedPlate { get; private set; } = null;

    private ItemSystem itemSystem;

    void Start()
    {
        itemSystem = GetComponent<ItemSystem>();
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
    
    public bool TryAttachToPlate(PlateSystem plate)
    {
        if (isAttachedToPlate) return false;
        
        isAttachedToPlate = true;
        attachedPlate = plate;
        return true;
    }
    
    public void DetachFromPlate()
    {
        isAttachedToPlate = false;
        attachedPlate = null;
    }
}