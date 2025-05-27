using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HpFollow : MonoBehaviour
{
    public Transform FollowWho;

    // Update is called once per frame
    void Update()
    {
        if (FollowWho == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = FollowWho.position;
    }
}
