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

    void Awake()
    {
        transition = GetComponentInChildren<Animator>();

        transition.SetBool("PlayTransitionOnStart", playTransitionOnStart);
    }

    void Start()
    {
        if (!playTransitionOnStart) transition.gameObject.SetActive(false);
    }

    public void LoadSceneWithTransition(string SceneName)
    {
        transition.gameObject.SetActive(true);
        StartCoroutine(LoadLevel(SceneName));
    }

    IEnumerator LoadLevel(string SceneName)
    {
        transition.SetTrigger("StartTransition");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(SceneName);
    }
}
