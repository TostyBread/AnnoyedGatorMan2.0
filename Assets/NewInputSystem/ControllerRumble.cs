using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerRumble : MonoBehaviour
{
    // TO CALL CONTROLLER RUMBLE, use:
    // ControllerRumble.Instance.StartRumble(0.7f, 0.7f, 1f);
    public static ControllerRumble Instance { get; private set; }

    private float rumbleEndTime = 0f;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Gamepad.current != null && Time.time > rumbleEndTime)
        {
            StopRumble();
        }
    }
    /// Starts rumble with given intensities and duration
    public void StartRumble(float lowFrequency, float highFrequency, float duration)
    {
        if (Gamepad.current == null)
        {
            Debug.LogWarning("No gamepad connected.");
            return;
        }

        Gamepad.current.SetMotorSpeeds(lowFrequency, highFrequency);
        rumbleEndTime = Time.time + duration;
    }
    /// Stops rumble immediately
    public void StopRumble()
    {
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(0, 0);
        }
    }
}