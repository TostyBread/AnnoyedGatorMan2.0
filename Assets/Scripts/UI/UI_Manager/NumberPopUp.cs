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
    public GameObject numberGameObject;
    private TMP_Text servedCustomerText;
    private Animator animaator;

    private float currentMoveTime;
    private Coroutine currentCoroutine;
    private ScoreManager scoreManager;
    private int lastScore = 0;
    private Vector3 initialPosition;

    void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
        if (numberGameObject != null) { servedCustomerText = numberGameObject.GetComponentInChildren<TMP_Text>(); }
        animaator = GetComponentInChildren<Animator>();

        initialPosition = transform.position;
        numberGameObject.gameObject.SetActive(false);
    }

    void Update()
    {
        if (numberGameObject.gameObject.activeSelf)
        {
            if (currentMoveTime < moveDuration)
            {
                numberGameObject.transform.position += new Vector3(0, moveSpeedY) * Time.deltaTime;
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
        numberGameObject.transform.position = initialPosition;
        servedCustomerText.text = scoreManager.currentScore.ToString();
        numberGameObject.gameObject.SetActive(true);
        animaator.SetTrigger("ScoreUp");

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
        numberGameObject.gameObject.SetActive(false);
    }
}