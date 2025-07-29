using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public float MaxTime = 10;
    public float RemainTime;
    public Image TimerBar;

    private TMP_Text TimerText;
    private CameraMovement cameraMovement;

    // Start is called before the first frame update
    void Start()
    {
        RemainTime = MaxTime;
        TimerText = GetComponentInChildren<TMP_Text>();
        cameraMovement = FindObjectOfType<CameraMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (TimerBar != null) TimerBar.fillAmount = RemainTime / MaxTime;

        //Convert RemainTime to minutes and seconds
        int minutes = Mathf.FloorToInt(RemainTime / 60f);
        int seconds = Mathf.FloorToInt(RemainTime % 60f);
        if (TimerText != null) TimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (cameraMovement != null && !cameraMovement.isMoving)
        {
            StartCoroutine(Wait(1f)); // Wait before reducing the time
        }
        else if (cameraMovement == null)
        {
            RemainTime -= Time.deltaTime;
        }

        RemainTime = Mathf.Clamp(RemainTime, 0, MaxTime);

        if (Input.GetKeyDown(KeyCode.Delete)) RemainTime = 0; // For testing purposes
    }

    IEnumerator Wait(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        RemainTime -= Time.deltaTime;
    }
}
