using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TotalInterruption : MonoBehaviour
{
    private int interruptionCount = 0;

    public void IncrementInterruption()
    {
        interruptionCount++;
        Debug.Log($"Total interruptions: {interruptionCount}");
    }
}
