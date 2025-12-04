using UnityEngine;

public class MenuOnHover : MonoBehaviour
{
    public GameObject instructionMenu;
    protected bool isHovering = false;

    void Start()
    {
        instructionMenu.SetActive(false); // Ensure the menu is hidden at the start
        isHovering = false;
    }

    void OnMouseEnter()
    {
        isHovering = true;
        instructionMenu.SetActive(true);
    }

    void OnMouseExit()
    {
        isHovering = false;
        instructionMenu.SetActive(false);
    }
}
