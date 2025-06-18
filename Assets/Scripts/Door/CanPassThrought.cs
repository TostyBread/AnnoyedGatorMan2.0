using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanPassThrought : MonoBehaviour
{
    public DoNotHurt doNotHurt;

    void Update()
    {
        if (doNotHurt != null && doNotHurt.doNotHurt != null)
        {
            Collider2D myCollider = GetComponent<Collider2D>();
            Collider2D otherCollider = doNotHurt.doNotHurt.GetComponent<Collider2D>();

            if (myCollider != null && otherCollider != null)
            {
                Physics2D.IgnoreCollision(myCollider, otherCollider);
            }
        }
    }
}
