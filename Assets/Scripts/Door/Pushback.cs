using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pushback : MonoBehaviour
{
    public DoorHitBox[] doorHitBox;
    public List<GameObject> allTargetInDoorHitbox = new List<GameObject>();
    List<HealthManager> colHealthManager = new List<HealthManager>();

    public static event System.Action OnPushbackFinished;

    private void OnEnable()
    {
        // Enable collisions with targets in list
        EnableOnlyTargetCollisions();

        StartCoroutine(DisableAfterPush(0.5f));
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!allTargetInDoorHitbox.Contains(col.gameObject))
        {
            // Ignore collision with non-targets
            Collider2D myCollider = GetComponent<Collider2D>();
            Collider2D otherCollider = col.collider;

            if (myCollider != null && otherCollider != null)
            {
                Physics2D.IgnoreCollision(myCollider, otherCollider, true);
            }
        }


        if (col.gameObject.TryGetComponent(out HealthManager _colHealthManager))
        {
            colHealthManager.Add(_colHealthManager);
        }
        
    }

    private void EnableOnlyTargetCollisions()
    {
        Collider2D myCollider = GetComponent<Collider2D>();
        Collider2D[] allColliders = FindObjectsOfType<Collider2D>();

        foreach (Collider2D col in allColliders)
        {
            if (myCollider == col) continue;

            bool isTarget = allTargetInDoorHitbox.Contains(col.gameObject);

            // Allow only collisions with targets
            Physics2D.IgnoreCollision(myCollider, col, !isTarget);
        }
    }

    protected IEnumerator DisableAfterPush(float time)
    {
        yield return new WaitForSeconds(time);

        allTargetInDoorHitbox.Clear();

        // Re-enable all collisions before disabling
        EnableAllCollisions();

        foreach (HealthManager hm in colHealthManager)
        {
            hm.canMove = true;
        }

        if (OnPushbackFinished != null)
            OnPushbackFinished.Invoke();

        gameObject.SetActive(false);
    }

    private void EnableAllCollisions()
    {
        Collider2D myCollider = GetComponent<Collider2D>();
        Collider2D[] allColliders = FindObjectsOfType<Collider2D>();

        foreach (Collider2D col in allColliders)
        {
            if (myCollider == col) continue;
            Physics2D.IgnoreCollision(myCollider, col, false);
        }
    }
}