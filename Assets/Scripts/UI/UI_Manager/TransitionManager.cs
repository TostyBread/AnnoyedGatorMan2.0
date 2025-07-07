using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{
    public float transitionTime = 1f;
    public bool playTransitionOnStart = true;

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

    public void LoadSceneWithTransition(string SceneName)
    {
        transition.gameObject.SetActive(true);
        if (!isTransitioning) StartCoroutine(LoadLevel(SceneName));
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
