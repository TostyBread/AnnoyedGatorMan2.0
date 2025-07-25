using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoNotHurt : MonoBehaviour
{
    public GameObject doNotHurt;

    public DoorManager[] doors;
    public DoorHitBox[] hitboxes;

    private Collider2D storedCol;
    private Collider2D storedHitboxCol;

    //void OnEnable()
    //{
    //    Pushback.OnPushbackFinished += ReenableCollisionFromPushback;
    //}

    //void OnDisable()
    //{
    //    Pushback.OnPushbackFinished -= ReenableCollisionFromPushback;
    //}

    void ReenableCollisionFromPushback()
    {
        if (storedCol != null && storedHitboxCol != null)
            Physics2D.IgnoreCollision(storedCol, storedHitboxCol, false);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (DoorManager door in doors) 
        {
            DoorManager _door = door;
            if (_door.DoNotHurt != null)
            {
                //get the DoNotHurt from DoorManager
                doNotHurt = door.DoNotHurt;

                //and remove the DoNotHurt at DoorManager
                door.DoNotHurt = null;

                if (doNotHurt != null && doNotHurt.TryGetComponent(out Collider2D col))
                {
                    for (int i = 0; i < hitboxes.Length; i++)
                    {
                        if (hitboxes[i] != null && hitboxes[i].TryGetComponent(out Collider2D hitboxCol))
                        {
                            storedCol = col;
                            storedHitboxCol = hitboxCol;
                            Physics2D.IgnoreCollision(col, hitboxCol, true);
                        }
                    }
                }

                break;
            }
        }
    }

    public void RegisterDoNotHurt(GameObject obj)
{
    if (obj != null && obj.TryGetComponent(out Collider2D col))
    {
        foreach (var hitbox in hitboxes)
        {
            if (hitbox != null && hitbox.TryGetComponent(out Collider2D hitboxCol))
            {
                Physics2D.IgnoreCollision(col, hitboxCol, true);
                StartCoroutine(ReenableCollisionAfterDelay(col, hitboxCol, 0.1f));
            }
        }
    }
}

    private IEnumerator ReenableCollisionAfterDelay(Collider2D a, Collider2D b, float delay)
    {
        yield return new WaitForSeconds(delay);
        Physics2D.IgnoreCollision(a, b, false);
    }
}
