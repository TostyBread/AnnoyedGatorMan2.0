using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoNotHurt : MonoBehaviour
{
    public GameObject doNotHurt;

    public DoorManager[] doors;
    public DoorHitBox[] hitboxes;

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
                            Physics2D.IgnoreCollision(col, hitboxCol);
                        }
                    }
                }

                break;
            }
        }
    }
}
