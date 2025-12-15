using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DpadButtonTrigger : MonoBehaviour
{
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    [SerializeField] private float stickDeadzone = 0.5f;
    [SerializeField] private float repeatDelay = 0.25f;

    private float _lastTriggerTime;

    private void Update()
    {
        if (Gamepad.current == null)
            return;

        if (Time.unscaledTime - _lastTriggerTime < repeatDelay)
            return;

        // D-Pad
        if (Gamepad.current.dpad.left.wasPressedThisFrame)
            TriggerLeft();

        if (Gamepad.current.dpad.right.wasPressedThisFrame)
            TriggerRight();

        // Left Stick
        float stickX = Gamepad.current.leftStick.x.ReadValue();

        if (stickX <= -stickDeadzone)
            TriggerLeft();

        if (stickX >= stickDeadzone)
            TriggerRight();
    }

    private void TriggerLeft()
    {
        Trigger(leftButton);
        _lastTriggerTime = Time.unscaledTime;
    }

    private void TriggerRight()
    {
        Trigger(rightButton);
        _lastTriggerTime = Time.unscaledTime;
    }

    private static void Trigger(Button button)
    {
        if (button == null || !button.interactable)
            return;

        button.onClick.Invoke();
    }
}