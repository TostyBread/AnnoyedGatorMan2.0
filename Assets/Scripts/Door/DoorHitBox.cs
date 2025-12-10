using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorHitBox : MonoBehaviour
{
    public float damage;
    public List<GameObject> targets = new List<GameObject>();

    public GameObject pushback;
    private Pushback pushbackScript;

    private void Awake()
    {
        if (pushback != null)
            pushbackScript = pushback.GetComponent<Pushback>();
    }

    private void OnEnable()
    {
        //set end time
        Debug.Log("enable");
        StartCoroutine(DisableAfter(0.1f));
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("DoorHitBox triggered by: " + col.name);

        if (col.gameObject.CompareTag("Player"))
        {
            if (col.gameObject.TryGetComponent(out PlayerThrowManager colThrowManager) &&
                col.gameObject.TryGetComponent(out HealthManager colHealthManager))
            {
                //colThrowManager.doorCauseThrow = true;
                //colThrowManager.StartPreparingThrow(); Not using anymore
                colThrowManager.Throw();

                //stun the player from moving
                colHealthManager.canMove = false;

                colHealthManager.TryDamage(damage, this.gameObject);

                //colThrowManager.doorCauseThrow = false;
            }
        }
        else if (col.gameObject.CompareTag("Enemy"))
        {
            if (col.gameObject.TryGetComponent(out HealthManager colHealthManager))
            {
                colHealthManager.TryDamage(damage, this.gameObject);
            }
        }

        if (!targets.Contains(col.gameObject))
        {
            targets.Add(col.gameObject);
            pushbackScript.allTargetInDoorHitbox.Add(col.gameObject);
        }

        StartCoroutine(EnablePushback());
    }

    private IEnumerator EnablePushback()
    {
        if (targets.Count > 0)
            Debug.Log("Hit targets: " + targets.Count);

        if (pushback != null)
        {
            Debug.Log("Enabling pushback");
            pushback.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Pushback is null!");
        }

        yield return new WaitForEndOfFrame();

        gameObject.SetActive(false);
    }

    IEnumerator DisableAfter(float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }
}