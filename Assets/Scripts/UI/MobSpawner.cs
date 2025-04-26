using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    public GameObject mob;
    public Timer timer;
    public GameObject spawnPos;

    private bool mobSpawned = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (timer == null) return;

        if (timer.RemainTime == 0 && mobSpawned == false)
        {
            GameObject.Instantiate(mob, spawnPos.transform.position, spawnPos.transform.rotation);
            mobSpawned = true;
        }
    }
}
