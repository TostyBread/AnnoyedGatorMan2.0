using System.Collections;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Vector2 cameraStrtPosition;
    public Vector2 cameraEndPosition;

    public float cameraMoveTime = 1f;
    public bool isMoving = false; // Used by timer

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector2(cameraStrtPosition.x, cameraStrtPosition.y);

        MoveTowards(cameraEndPosition);
    }

    public void MoveTowards(Vector2 targetPosition)
    {
        StartCoroutine(MoveCamera(targetPosition));
    }

    private IEnumerator MoveCamera(Vector2 targetPosition)
    {
        isMoving = true;
        Vector2 startPosition = transform.position;
        float elapsedTime = 0f;
        while (elapsedTime < cameraMoveTime)
        {
            transform.position = Vector2.Lerp(startPosition, targetPosition, elapsedTime / cameraMoveTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = new Vector2(targetPosition.x, targetPosition.y);
        isMoving = false;
    }
}
