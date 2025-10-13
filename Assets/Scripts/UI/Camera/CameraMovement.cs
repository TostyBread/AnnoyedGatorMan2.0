using System.Collections;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Camera Setting")]
    public Vector2 cameraStrtPosition;
    public Vector2 cameraEndPosition;

    public float cameraMoveTime = 1f;
    public bool isMoving = false; // Used by timer

    [Header("Dialogue Setting")]
    public float delayBeforeDialogue = 0.5f;
    public GameObject dialogueTrigger;

    // Start is called before the first frame update
    void Start()
    {
        //dialogueTrigger = GameObject.FindGameObjectWithTag("DialogueTrigger");
        if (dialogueTrigger != null) dialogueTrigger.SetActive(false);

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

        if (dialogueTrigger != null)
        {
            yield return new WaitForSeconds(delayBeforeDialogue);

            dialogueTrigger.SetActive(true);
        }
    }
}
