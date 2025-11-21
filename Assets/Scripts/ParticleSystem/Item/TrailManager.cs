using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailManager : MonoBehaviour
{
    private TrailRenderer trailRenderer;

    // Start is called before the first frame update
    void Start()
    {
        trailRenderer = GetComponent<TrailRenderer>();
        trailRenderer.emitting = false; // Disable trail emission at the start
                                        // Enable in PlayerThrowManager & ItemPackage
                                        // Disable in P2PickupSystem & PlayerPickupSystem
    }
}
