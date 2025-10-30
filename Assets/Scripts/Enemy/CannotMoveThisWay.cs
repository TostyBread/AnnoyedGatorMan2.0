using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks whether this grid point is blocked by relevant targets (player or food).
/// Uses a reference-count approach so multiple overlapping colliders don't incorrectly flip the state.
/// Keeps aimForFood in sync with parent EnemyMovement.
/// </summary>
public class CannotMoveThisWay : MonoBehaviour
{
    [SerializeField] public bool canMoveThisWay = true;
    private bool aimForFood;
    private int blockingCount = 0;

    private EnemyMovement parentEM;

    private void Awake()
    {
        parentEM = GetComponentInParent<EnemyMovement>();
    }

    private void Start()
    {
        if (parentEM == null)
            Debug.LogWarning($"{name}: Cannot find EnemyMovement in parents.");
        aimForFood = parentEM != null && parentEM.aimForFood;
        canMoveThisWay = true;
    }

    private void Update()
    {
        // Keep aimForFood in sync in case the enemy switches targets at runtime
        if (parentEM != null)
            aimForFood = parentEM.aimForFood;
    }

    private bool IsRelevantCollider(GameObject go)
    {
        if (go == null) return false;

        if (!aimForFood)
            return go.CompareTag("Player");
        

        // aimForFood true -> check food tags
        return go.CompareTag("FoodBig") || go.CompareTag("FoodSmall");
    }

    private void HandleEnter(GameObject go)
    {
        if (!IsRelevantCollider(go)) return;

        //If enemy is aiming for food, do NOT block movement immediately
        if (aimForFood && (go.CompareTag("FoodBig") || go.CompareTag("FoodSmall")))
            return;

        blockingCount++;
        if (blockingCount == 1)
            canMoveThisWay = false;
    }

    private void HandleExit(GameObject go)
    {
        if (!IsRelevantCollider(go)) return;

        blockingCount = Mathf.Max(0, blockingCount - 1);
        if (blockingCount == 0)
            canMoveThisWay = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleEnter(collision.gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        HandleExit(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleEnter(collision.gameObject);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        HandleExit(collision.gameObject);
    }
}