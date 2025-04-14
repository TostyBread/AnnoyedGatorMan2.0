using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    public enum PlayerState { Idle, Burn, Freeze, Stun }

    public PlayerState state;
    public float stateDur;

    private float idleMoveSpeed;
    private CharacterMovement characterMovement;

    // Start is called before the first frame update
    void Start()
    {
        characterMovement = GetComponent<CharacterMovement>();
        idleMoveSpeed = characterMovement.moveSpeed;

        state = PlayerState.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        if (characterMovement == null)
        {
            return;
        }

        if (state == PlayerState.Idle)
        {
            characterMovement.moveSpeed = idleMoveSpeed;
        }

        if (Input.GetKeyDown(KeyCode.Space) && state == PlayerState.Idle)
        {
            int randomState = Random.Range(1, 4);

            if ( randomState == 1)
            {
                StartCoroutine(Burn(stateDur));
            }

            if (randomState == 2)
            {
                StartCoroutine(Freeze(stateDur));
            }

            if (randomState == 3)
            {
                StartCoroutine(Stun(stateDur));
            }
        }
    }

    IEnumerator Burn(float dur)
    {
        state = PlayerState.Burn;

        float elapsed = 0f;
        float interval = 0.5f;

        while (elapsed < dur)
        {
            Vector2 randomMovement = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            characterMovement.SetMovement(randomMovement);

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        state = PlayerState.Idle;
    }

    IEnumerator Freeze(float dur)
    {
        state = PlayerState.Freeze;
        characterMovement.moveSpeed = 1;
        yield return new WaitForSeconds(dur);
        state = PlayerState.Idle;
    }

    IEnumerator Stun(float dur)
    {
        state = PlayerState.Stun;
        characterMovement.moveSpeed = 0;
        yield return new WaitForSeconds(dur);
        state = PlayerState.Idle;
    }
}
