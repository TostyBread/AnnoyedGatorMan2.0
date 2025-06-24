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
        Debug.Log("Pushback: OnEnable called");
        StartCoroutine(DisableAfterPush());

        // Enable collisions with targets in list
        EnableOnlyTargetCollisions();
    }

    private void Start()
    {
        StartCoroutine(DisableAfterPush());
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

    private IEnumerator DisableAfterPush()
    {
        Debug.Log("Pushback: Starting disable coroutine");

        yield return new WaitForSeconds(0.5f);

        Debug.Log("Pushback: Disabling now");

        // reset collisions
        foreach (GameObject target in allTargetInDoorHitbox)
        {
            if (target != null)
            {
                Physics2D.IgnoreCollision(target.GetComponent<Collider2D>(), GetComponent<Collider2D>(), false);
            }
        }

        allTargetInDoorHitbox.Clear();

        foreach (HealthManager hm in colHealthManager)
        {
            if (hm != null)
            {
                hm.canMove = true;
            }
        }

        colHealthManager.Clear();

        OnPushbackFinished?.Invoke();
        gameObject.SetActive(false);  // <--- THIS is the important line
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