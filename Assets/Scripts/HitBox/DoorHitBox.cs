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
        StartCoroutine(DisableAfter(0.1f));
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            if (col.gameObject.TryGetComponent(out PlayerThrowManager colThrowManager) &&
                col.gameObject.TryGetComponent(out HealthManager colHealthManager))
            {
                colThrowManager.doorCauseThrow = true;
                colThrowManager.StartPreparingThrow();
                colThrowManager.Throw();

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
        yield return new WaitForFixedUpdate();

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
