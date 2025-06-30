using UnityEngine;

[CreateAssetMenu(menuName = "Input/Player Input Config")]
public class PlayerInputConfigSO : ScriptableObject
{
    public KeyCode moveUp;
    public KeyCode moveDown;
    public KeyCode moveLeft;
    public KeyCode moveRight;

    public KeyCode attackKey;
    public KeyCode pickupKey;
    public KeyCode toggleSafetyKey;
    public KeyCode interactKey;
    public KeyCode throwPrepareKey;
    public KeyCode throwConfirmKey;

    public string joystickHorizontalAxis;
    public string joystickVerticalAxis;
}