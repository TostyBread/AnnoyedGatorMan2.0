using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NumberPopUp : MonoBehaviour
{
    public TMP_Text numberText;
    public float moveSpeedY = 1f;

    public float LifeTime = 1f;
    private float currentTime;

    private ScoreManager scoreManager;
    private int lastScore = 0;

    // Start is called before the first frame update
    void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentTime < LifeTime)
        {
            numberText.transform.position += new Vector3(0, moveSpeedY) * Time.deltaTime;
            currentTime += Time.deltaTime;
        }
        else
        {
            numberText.gameObject.SetActive(false);
        }

        if (lastScore != scoreManager.currentScore)
        {
            numberText.gameObject.SetActive(true);
            numberText.transform.position = transform.position;
            numberText.text = scoreManager.currentScore.ToString();

            lastScore = scoreManager.currentScore;
            currentTime = 0f;
        }
    }
}
