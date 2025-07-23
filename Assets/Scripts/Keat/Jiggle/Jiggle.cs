using System.Collections;
using UnityEngine;

public class Jiggle : MonoBehaviour
{
    // Combined Jiggle for position + rotation
    public bool jiggle;

    [Header("Common Settings")]
    public float jiggleInterval = 0.3f;
    public float BiggerTheGameobjectBy = 1.3f;

    [Header("Position Jiggle")]
    public bool enableLeftRightJiggle = true;
    public bool enableUpDownJiggle = false;
    public float jiggleRange = 0.5f;
    public float jiggleSpeed = 5f;

    [Header("Rotation Jiggle")]
    public bool enableRotationJiggle = true;
    public float rotationAngle = 30f;

    private bool once = true;

    private Vector3 defaultPosition;
    private Quaternion defaultRotation;
    private Vector3 defaultScale;

    private Coroutine jiggleRoutine;

    void Update()
    {
        if (jiggle && once)
        {
            // Save original transform
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
        // Define positions
        Vector3 leftPos = defaultPosition + Vector3.left * jiggleRange;
        Vector3 rightPos = defaultPosition + Vector3.right * jiggleRange;
        Vector3 upPos = defaultPosition + Vector3.up * jiggleRange;
        Vector3 downPos = defaultPosition + Vector3.down * jiggleRange;

        // Define rotations
        Quaternion leftRot = Quaternion.Euler(defaultRotation.eulerAngles + new Vector3(0, 0, rotationAngle));
        Quaternion rightRot = Quaternion.Euler(defaultRotation.eulerAngles + new Vector3(0, 0, -rotationAngle));

        // Left or Up
        if (enableLeftRightJiggle)
            yield return MoveToPosition(leftPos, jiggleSpeed);
        else if (enableUpDownJiggle)
            yield return MoveToPosition(upPos, jiggleSpeed);

        if (enableRotationJiggle)
            transform.rotation = leftRot;

        yield return new WaitForSeconds(interval);

        // Right or Down
        if (enableLeftRightJiggle)
            yield return MoveToPosition(rightPos, jiggleSpeed);
        else if (enableUpDownJiggle)// back to default here for up-down jiggle
        {
            yield return MoveToPosition(defaultPosition, jiggleSpeed); 
            ResetTransform();
        }

        if (enableRotationJiggle)
            transform.rotation = rightRot;

        yield return new WaitForSeconds(interval);

        // Return to center
        yield return MoveToPosition(defaultPosition, jiggleSpeed);
        transform.rotation = defaultRotation;

        yield return new WaitForSeconds(interval);

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

    private void ResetTransform()
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