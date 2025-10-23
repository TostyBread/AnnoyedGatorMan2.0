using System.Collections;
using UnityEngine;

public class Jiggle : MonoBehaviour
{
    [Header("Common Settings")]
    public float jiggleInterval = 0.3f;
    public float BiggerTheGameobjectBy = 1.3f;

    [Header("Position Jiggle")]
    public bool enableLeftRightJiggle;
    public bool enableUpDownJiggle;
    public float jiggleRange = 0.5f;
    public float jiggleSpeed = 5f;

    [Header("Rotation Jiggle")]
    public bool enableRotationJiggle;
    public float rotationAngle = 30f;

    private Vector3 defaultPosition;
    private Quaternion defaultRotation;
    private Vector3 defaultScale;

    private Coroutine jiggleRoutine;
    public Transform spriteGameobject;

    public void StartJiggle()
    {
        if (jiggleRoutine != null) return;

        if (spriteGameobject != null)
        {
            Debug.Log("Using spriteGameobject for jiggle.");
            defaultPosition = spriteGameobject.position;
            defaultRotation = spriteGameobject.rotation;
            defaultScale = spriteGameobject.localScale;
        }
        else
        {
            defaultPosition = transform.position;
            defaultRotation = transform.rotation;
            defaultScale = transform.localScale;
        }

        PopUp(BiggerTheGameobjectBy);
        jiggleRoutine = StartCoroutine(JiggleRoutine(jiggleInterval));
    }

    private IEnumerator JiggleRoutine(float interval)
    {
        // Define positions
        Vector3 leftPos = defaultPosition + Vector3.left * jiggleRange;
        Vector3 rightPos = defaultPosition + Vector3.right * jiggleRange;
        Vector3 upPos = defaultPosition + Vector3.up * jiggleRange;

        // Define rotations
        Quaternion leftRot = Quaternion.Euler(defaultRotation.eulerAngles + new Vector3(0, 0, rotationAngle));
        Quaternion rightRot = Quaternion.Euler(defaultRotation.eulerAngles + new Vector3(0, 0, -rotationAngle));

        // First movement: Left or Up
        if (enableLeftRightJiggle)
            yield return MoveToPosition(leftPos, jiggleSpeed);
        else if (enableUpDownJiggle)
            yield return MoveToPosition(upPos, jiggleSpeed);

        if (enableRotationJiggle)
            transform.rotation = leftRot;

        yield return new WaitForSeconds(interval);

        // Second movement: Right or Back to center
        if (enableLeftRightJiggle)
            yield return MoveToPosition(rightPos, jiggleSpeed);
        else if (enableUpDownJiggle)
            yield return MoveToPosition(defaultPosition, jiggleSpeed);

        if (enableRotationJiggle)
            transform.rotation = rightRot;

        yield return new WaitForSeconds(interval);

        // Return to center
        yield return MoveToPosition(defaultPosition, jiggleSpeed);
        transform.rotation = defaultRotation;

        yield return new WaitForSeconds(interval);

        ResetTransform();

        jiggleRoutine = null; // Allow future jiggle calls
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
        if (spriteGameobject != null)
        {
            spriteGameobject.position = defaultPosition;
            spriteGameobject.rotation = defaultRotation;
            spriteGameobject.localScale = defaultScale;
        }
        else
        {
            transform.position = defaultPosition;
            transform.rotation = defaultRotation;
            transform.localScale = defaultScale;
        }
    }

    private void PopUp(float enlargeSize)
    {
        if (spriteGameobject != null)
        {
            spriteGameobject.localScale = defaultScale * enlargeSize;
        }
        else
        {
            transform.localScale = defaultScale * enlargeSize;
        }
    }
}