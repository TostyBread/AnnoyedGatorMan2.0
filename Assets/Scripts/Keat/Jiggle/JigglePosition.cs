using System.Collections;
using UnityEngine;

public class JigglePosition : MonoBehaviour
{
    //This is jiggle for transform.position
    //How to use: 
    //1) Plug this code to the gameObject that want to jiggle
    //2) Use the gameObject to set this jiggle(boolean) to true
    //3) the gameObject will jiggle

    public bool jiggle;

    public float jiggleInterval = 0f;
    public float BiggerTheGameobjectBy = 1f;
    public float jiggleRange = 0.5f;
    public float jiggleSpeed = 5f;

    private bool once = true;

    private Vector3 defaultPosition;
    private Quaternion defaultRotation;
    private Vector3 defaultScale;

    private Coroutine jiggleRoutine;

    void Update()
    {
        if (jiggle && once)
        {
            //save the value of current gameObject before jiggle
            defaultPosition = transform.position;
            defaultRotation = transform.rotation;
            defaultScale = transform.localScale;

            once = false;
            jiggleRoutine = StartCoroutine(JiggleRoutine(jiggleInterval));
            PopUp(BiggerTheGameobjectBy);
        }

        if (!jiggle && !once)
        {
            if (jiggleRoutine != null)
                StopCoroutine(jiggleRoutine);

            ResetTransform();
            once = true;
        }
    }

    private IEnumerator JiggleRoutine(float interval)
    {
        Vector3 leftTarget = defaultPosition + Vector3.left * jiggleRange;
        Vector3 rightTarget = defaultPosition + Vector3.right * jiggleRange;

        // Left Jiggle
        yield return MoveToPosition(leftTarget, jiggleSpeed);
        yield return new WaitForSeconds(interval);

        // Right Jiggle
        yield return MoveToPosition(rightTarget, jiggleSpeed);
        yield return new WaitForSeconds(interval);

        // Return to center
        yield return MoveToPosition(defaultPosition, jiggleSpeed);
        yield return new WaitForSeconds(interval);

        // Reset
        ResetTransform();
    }

    private IEnumerator MoveToPosition(Vector3 target, float speed)
    {
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speed);
            yield return null;
        }
    }

    private void ResetTransform() //here is where this code end jiggle
    {
        transform.position = defaultPosition;
        transform.rotation = defaultRotation;
        transform.localScale = defaultScale;

        jiggle = false;
    }

    private void PopUp(float enlargeSize)
    {
        transform.localScale = defaultScale * enlargeSize;
    }
}