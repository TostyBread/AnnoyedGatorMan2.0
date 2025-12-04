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
    public GameObject bonusGameObject;

    private TMP_Text servedCustomerText;
    private TMP_Text bonusText;
    private Animator animator;

    private float currentMoveTime;
    private Coroutine currentCoroutine;
    private ScoreManager scoreManager;
    private int lastScore = 0;
    private Vector3 initialPosition;
    private Jiggle jiggle;

    void Start()
    {
        jiggle = GetComponent<Jiggle>();
        scoreManager = FindObjectOfType<ScoreManager>();
        if (numberGameObject != null) { servedCustomerText = numberGameObject.GetComponentInChildren<TMP_Text>(); }
        if (bonusGameObject != null) { bonusText = bonusGameObject.GetComponentInChildren<TMP_Text>(); }
        animator = GetComponentInChildren<Animator>();

        initialPosition = transform.position;
        numberGameObject.gameObject.SetActive(false);
        bonusGameObject.gameObject.SetActive(false);
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
        jiggle.StartJiggle();

        AudioManager.Instance.PlaySound("KaChing", transform.position); // Play SFX

        // Reset position and state
        initialPosition = transform.position;
        numberGameObject.transform.position = initialPosition;

        // Update score into TMP_text
        servedCustomerText.text = scoreManager.currentScore.ToString();
        bonusText.text = scoreManager.serveCombo.ToString();

        // Show score
        numberGameObject.gameObject.SetActive(true);
        bonusGameObject.gameObject.SetActive(true);
        animator.SetTrigger("ScoreUp");

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
        bonusGameObject.gameObject.SetActive(false);
    }
}