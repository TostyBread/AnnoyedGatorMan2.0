using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magnifier : MonoBehaviour
{
    public GameObject sunRay;
    public bool underSun;
    public GameObject BurningSunRay;

    private CapsuleCollider2D capsuleCollider;

    // Start is called before the first frame update
    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        BurningSunRay.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        BurningSunRay.SetActive(underSun);
        if (!capsuleCollider.enabled) capsuleCollider.enabled = true;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject == sunRay)
        {
            underSun = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == sunRay)
        {
            underSun = false;
        }
    }
}
