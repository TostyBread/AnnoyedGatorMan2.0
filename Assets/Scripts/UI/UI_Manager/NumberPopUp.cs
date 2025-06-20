using UnityEngine;
using TMPro;
using System.Collections;

public class NumberPopUp : MonoBehaviour
{
    [Header("Pop Up Setting")]
    public float moveSpeedY = 1f;
    public float moveDuration = 0.5f; // Time moving upward
    public float fixedDisplayTime = 0.5f; // Time staying still before disappearing

    [Header("References")]
    public TMP_Text numberText;

    private float currentMoveTime;
    private Coroutine currentCoroutine;
    private ScoreManager scoreManager;
    private int lastScore = 0;
    private Vector3 initialPosition;

    void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
        initialPosition = transform.position;
        numberText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (numberText.gameObject.activeSelf)
        {
            if (currentMoveTime < moveDuration)
            {
                numberText.transform.position += new Vector3(0, moveSpeedY) * Time.deltaTime;
                currentMoveTime += Time.deltaTime;
            }
        }

        if (lastScore != scoreManager.currentScore)
        {
            TriggerPopUp();
            lastScore = scoreManager.currentScore;
        }
    }

    void TriggerPopUp()
    {
        // Reset position and state
        initialPosition = transform.position;
        numberText.transform.position = initialPosition;
        numberText.text = scoreManager.currentScore.ToString();
        numberText.gameObject.SetActive(true);

        currentMoveTime = 0f;

        // Stop any existing coroutine
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        // Start new disappearance timer
        currentCoroutine = StartCoroutine(DisappearAfterTime());
    }

    IEnumerator DisappearAfterTime()
    {
        yield return new WaitForSeconds(moveDuration + fixedDisplayTime);
        numberText.gameObject.SetActive(false);
    }
}