using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Xml.Serialization;

public class LevelLoader : MonoBehaviour
{
    [Header("Menu Screens")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject mainMenu;

    [Header("Other UI Screens")]
    [SerializeField] private GameObject playerNumberScreen;
    [SerializeField] private GameObject settingScreen;
    [SerializeField] private GameObject ShaderScreen;

    [Header("Slider")]
    [SerializeField] private Slider loadingSlider;

    private void Start()
    {
        if (mainMenu) mainMenu.SetActive(true);
        if (loadingScreen) loadingScreen.SetActive(false);

        if (playerNumberScreen) playerNumberScreen.SetActive(false);
        if (settingScreen) settingScreen.SetActive(false);
        if (ShaderScreen) ShaderScreen.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Quit();
    }

    public void LoadLevelButton(string levelToLoad)
    {
        if (mainMenu) mainMenu.SetActive(false);
        if (loadingScreen) loadingScreen.SetActive(true);

        //Run Async
        StartCoroutine(LoadLevelSync(levelToLoad));
    }

    public void ShowPopUpScreen()
    {
        playerNumberScreen.SetActive(true);
        ShaderScreen.SetActive(true);
    }

    public void ShowSettingScreen()
    {
        settingScreen.SetActive(true);
        ShaderScreen.SetActive(true);
    }

    public void Quit()
    {
        Application.Quit();
    }

    IEnumerator LoadLevelSync(string levelToLoad)
    {
        if (playerNumberScreen) playerNumberScreen.SetActive(false); 
        if (ShaderScreen) ShaderScreen.SetActive(false);

        if (loadingScreen) yield return new WaitForSeconds(3f);

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);

        while (!loadOperation.isDone)
        {
            float progressValue = Mathf.Clamp01(loadOperation.progress / 0.9f);
            if (loadingSlider) loadingSlider.value = progressValue;
            yield return null;
        }
    }
}
