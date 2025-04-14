using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    public enum PlayerState { Idle, Burn, Freeze, Stun }

    public PlayerState state;
    private CharacterMovement characterMovement;
    private PlayerInputManager playerInputManager;

    private float idleMoveSpeed;

    // Start is called before the first frame update
    void Start()
    {
        characterMovement = GetComponent<CharacterMovement>();
        idleMoveSpeed = characterMovement.moveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (state == PlayerState.Idle)
        {
            characterMovement.moveSpeed = idleMoveSpeed;
        }

        if (state == PlayerState.Burn)
        {
            Vector2 randomMovement = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            GetComponent<CharacterMovement>()?.SetMovement(randomMovement);
        }

        if (state == PlayerState.Freeze)
        {
            characterMovement.moveSpeed = 1;
        }

        if (state == PlayerState.Stun)
        {
            // PlayerInputManager
        }
    }
}
