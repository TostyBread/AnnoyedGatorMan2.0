using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTowards : MonoBehaviour
{
    private Dumpster dumpster;
    public Transform Target;
    private Jiggle dumpsterJiggle;
    private EnemySpawner enemySpawner;
    public enum Type
    { 
        enemyDeadBody,
        Trashbag,
        burntFood,
        none
    }
    public Type currentGameObject = Type.enemyDeadBody;

    public float speed = 5f;
    // Start is called before the first frame update
    void Start()
    {
        transform.parent = null;
        dumpster = FindAnyObjectByType<Dumpster>(); 
        enemySpawner = FindObjectOfType<EnemySpawner>();

        if (Target == null && dumpster != null) 
        {
            Target = dumpster.transform;
            dumpsterJiggle = Target.GetComponents<Jiggle>()[1];
        }


        if (Target == null)
            Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position,Target.position, Time.deltaTime*speed);

        if (Vector2.Distance(transform.position, Target.position) < 0.1f)
        {
            dumpsterJiggle.StartJiggle();

            if (currentGameObject == Type.enemyDeadBody)
                enemySpawner.EnemySpeedUpEnemySpawn();
            else if (currentGameObject == Type.Trashbag)
                enemySpawner.TrashbagSpeedUpEnemySpawn();
            else if (currentGameObject == Type.burntFood)
                enemySpawner.BurntFoodSpeedUpEnemySpawn();

            Destroy(this.gameObject);
        }
    }
}
