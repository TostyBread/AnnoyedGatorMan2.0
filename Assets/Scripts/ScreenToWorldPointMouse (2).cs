using UnityEngine;

public class ScreenToWorldPointMouse : MonoBehaviour
{
    public static ScreenToWorldPointMouse Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Function to get the mouse position in world space
    public Vector2 GetMouseWorldPosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
