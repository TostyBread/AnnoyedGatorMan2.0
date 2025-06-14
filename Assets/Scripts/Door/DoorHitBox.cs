using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        StartCoroutine(DisableAfter(0.1f));
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            if (col.gameObject.TryGetComponent(out PlayerThrowManager colThrowManager) &&
                col.gameObject.TryGetComponent(out HealthManager colHealthManager))
            {
                colThrowManager.doorCauseThrow = true;
                colThrowManager.StartPreparingThrow();
                colThrowManager.Throw();

                //stun the player from moving
                colHealthManager.canMove = false;

                colHealthManager.TryDamage(damage);

                colThrowManager.doorCauseThrow = false;
            }
        }
        else if (col.gameObject.CompareTag("Enemy"))
        {
            if (col.gameObject.TryGetComponent(out HealthManager colHealthManager))
            {
                colHealthManager.TryDamage(damage);
            }
        }

        if (!targets.Contains(col.gameObject))
        {
            targets.Add(col.gameObject);
            pushbackScript.allTargetInDoorHitbox.Add(col.gameObject);
        }

        StartCoroutine(DisableAfterHit());
    }

    IEnumerator DisableAfterHit()
    {
        yield return new WaitForSeconds(0.05f);

        if (targets.Count > 0)
            targets.Clear();

        if (pushback != null)
            pushback.SetActive(true);

        gameObject.SetActive(false);
    }

    IEnumerator DisableAfter(float time)
    { 
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }
}
