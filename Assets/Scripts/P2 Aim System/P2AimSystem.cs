using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P2AimSystem : MonoBehaviour
{
    private DetectTarget detectTarget;
    private int currentTargetIndex;
    public GameObject Range;


    // Start is called before the first frame update
    void Start()
    {
        detectTarget = Range.GetComponent<DetectTarget>();
    }

    // Update is called once per frame
    void Update()
    {
        ChangeTarget();

    }

    private GameObject NearestEnemy()
    {
        if (detectTarget.AllEnemyInRange.Count == 0)
        {
            return null;
        }

        // Ensure index is within bounds
        currentTargetIndex = Mathf.Clamp(currentTargetIndex, 0, detectTarget.AllEnemyInRange.Count - 1);

        return detectTarget.AllEnemyInRange[currentTargetIndex];
    }

    private void ChangeTarget()
    {
        if (detectTarget.AllEnemyInRange.Count > 0)
        {
            if (Input.GetKeyDown(KeyCode.Tab)) // Switch to the next enemy
            {
                currentTargetIndex = (currentTargetIndex + 1) % detectTarget.AllEnemyInRange.Count;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Tab)) // Switch to the previous enemy
            {
                currentTargetIndex--;
                if (currentTargetIndex < 0)
                    currentTargetIndex = detectTarget.AllEnemyInRange.Count - 1;
            }
        }
    }
}
