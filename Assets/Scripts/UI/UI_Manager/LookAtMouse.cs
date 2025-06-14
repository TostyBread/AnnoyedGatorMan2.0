using UnityEngine;

public class LookAtMouse : MonoBehaviour
{
    void Update()
    {
        // Get mouse position in world space
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = transform.position.z; // Ensure same z-position

        // Calculate direction
        Vector2 direction = mousePos - transform.position;

        // Calculate angle and convert to 0-360 range
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle = (angle + 360) % 360; // Ensure positive 0-360 value

        // Apply rotation with optional offset
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}