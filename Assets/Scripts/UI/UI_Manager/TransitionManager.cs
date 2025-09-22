using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{
    public float transitionTime = 1f;
    public bool playTransitionOnStart = true;
    public float transitionDelay = 0f;

    private Animator transition;
    public bool isTransitioning = false;

    void Awake()
    {
        transition = GetComponentInChildren<Animator>();

        transition.SetBool("PlayTransitionOnStart", playTransitionOnStart);
    }

    void Start()
    {
        if (playTransitionOnStart)
        {
            isTransitioning = true;
            StartCoroutine(HandleStartTransition());
        }
        else
        {
            transition.gameObject.SetActive(false);
            isTransitioning = false;
        }
    }

    IEnumerator HandleStartTransition()
    {
        // Ensure the object is active in case it was disabled
        transition.gameObject.SetActive(true);

        yield return new WaitForSeconds(transitionTime);

        isTransitioning = false;
        transition.gameObject.SetActive(false);
    }

    public void LoadSceneWithTransitionDelay(string SceneName)
    {
        if (!isTransitioning)
        {
            transition.gameObject.SetActive(true);
            StartCoroutine(LoadLevelWithDelay(SceneName));
        }
    }

    public void LoadSceneWithTransition(string SceneName)
    {
        if (!isTransitioning)
        {
            transition.gameObject.SetActive(true);
            StartCoroutine(LoadLevel(SceneName));
        }
    }

    IEnumerator LoadLevelWithDelay(string SceneName)
    {
        yield return new WaitForSeconds(transitionDelay);

        transition.SetTrigger("StartTransition");
        isTransitioning = true;

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(SceneName);
        isTransitioning = false;
    }

    IEnumerator LoadLevel(string SceneName)
    {
        transition.SetTrigger("StartTransition");
        isTransitioning = true;

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(SceneName);
        isTransitioning = false;
    }
}
