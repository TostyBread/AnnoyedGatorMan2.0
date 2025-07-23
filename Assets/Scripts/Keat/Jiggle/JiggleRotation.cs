using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JiggleRotation : MonoBehaviour
{
    //This is jiggle for transform.rotation
    //How to use: 
    //1) Plug this code to the gameObject that want to jiggle
    //2) Use the gameObject to set this jiggle(boolean) to true
    //3) the gameObject will jiggle
    public bool jiggle;

    public float jiggleInterval = 0.3f;
    public float BiggerTheGameobjectBy = 1.3f;

    private bool once = true;

    private Vector3 defaultPosition;
    private Quaternion defaultRotation;
    private Vector3 defaultScale;

    private Coroutine jiggleRoutine;

    // Update is called once per frame
    void Update()
    {
        if (jiggle && once)
        {
            //save the value of current gameObject before jiggle
            defaultPosition = transform.position;
            defaultRotation = transform.rotation;
            defaultScale = transform.localScale;

            once = false;
            jiggleRoutine = StartCoroutine(GiggleRoutine(jiggleInterval));
            PopUp(BiggerTheGameobjectBy);
        }

        if (!jiggle && !once)
        {
            // Reset when giggle ends
            if (jiggleRoutine != null) StopCoroutine(jiggleRoutine);

            once = true;
        }
    }

    private IEnumerator GiggleRoutine(float Interval)
    {
        // Jiggle rotation to left
        transform.rotation = Quaternion.Euler(defaultRotation.eulerAngles + new Vector3(0, 0, 30));

        yield return new WaitForSeconds(Interval);

        // Jiggle rotation to right
        transform.rotation = Quaternion.Euler(defaultRotation.eulerAngles + new Vector3(0, 0, -30));

        yield return new WaitForSeconds(Interval);

        ResetTransform();
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
        // Scale up and reset
        transform.localScale = defaultScale * enlargeSize;
    }
}

