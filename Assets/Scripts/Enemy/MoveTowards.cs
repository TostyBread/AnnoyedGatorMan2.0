using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTowards : MonoBehaviour
{
    public Transform Dumpster;
    public float speed = 5f;
    // Start is called before the first frame update
    void Start()
    {
        Dumpster = FindAnyObjectByType<Dumpster>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position,Dumpster.position, Time.deltaTime*speed);
    }
}
