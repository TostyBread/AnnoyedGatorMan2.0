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
    public bool isShowingSettingScreen = false;

    [Header("Slider")]
    [SerializeField] private Slider loadingSlider;

    [Header("Reference")]
    public LevelSelectManager levelSelectManager;

    private TransitionManager transitionManager;
    private ClickerDetector clickerDetector;
    private PauseManager pauseManager;

    private bool isLoading = false;

    private void Start()
    {
        transitionManager = FindObjectOfType<TransitionManager>();
        clickerDetector = FindObjectOfType<ClickerDetector>();
        pauseManager = FindObjectOfType<PauseManager>();

        if (mainMenu) mainMenu.SetActive(true);
        if (loadingScreen) loadingScreen.SetActive(false);

        if (playerNumberScreen) playerNumberScreen.SetActive(false);
        if (settingScreen) settingScreen.SetActive(false);
        if (ShaderScreen) ShaderScreen.SetActive(false);
    }

    private void Update()
    {
        if (settingScreen != null)
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !isLoading && !transitionManager.isTransitioning && !isShowingSettingScreen)
            {
                ShowSettingScreen();
            }
            else if (Input.GetKeyDown(KeyCode.Escape) && !isLoading && !transitionManager.isTransitioning && isShowingSettingScreen)
            {
                HideSettingtScreen();
            }
        }      
    }

    public void LoadLevelButton(string levelToLoad)
    {
        Time.timeScale = 1f;
        if (mainMenu) mainMenu.SetActive(false);
        if (loadingScreen) loadingScreen.SetActive(true);

        //Run Async
        if (levelSelectManager == null) StartCoroutine(LoadLevelSync(levelToLoad));
        else
        {
             StartCoroutine(LoadLevelSync(levelSelectManager.levelSelected));
        }
    }

    public void ShowPopUpScreen()
    {
        if (playerNumberScreen) playerNumberScreen.SetActive(true);
        if (ShaderScreen) ShaderScreen.SetActive(true);
    }

    public void ShowSettingScreen()
    {
        isShowingSettingScreen = true;
        if (pauseManager != null) pauseManager.PauseGame();

        settingScreen.SetActive(true);
        ShaderScreen.SetActive(true);
    }

    public void HideSettingtScreen()
    {
        isShowingSettingScreen = false;
        if (pauseManager != null) pauseManager.ResumeGame();
        if (clickerDetector != null) clickerDetector.ClosePopUpScreens();

        settingScreen.SetActive(false);
        ShaderScreen.SetActive(false);
    }

    public void ChangeScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void Quit()
    {
        Application.Quit();
    }

    IEnumerator LoadLevelSync(string levelToLoad)
    {
        isLoading = true;
        if (playerNumberScreen) playerNumberScreen.SetActive(false); 
        if (ShaderScreen) ShaderScreen.SetActive(false);

        if (loadingScreen) yield return new WaitForSeconds(3f);

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);

        while (!loadOperation.isDone)
        {
            float progressValue = Mathf.Clamp01(loadOperation.progress / 0.9f);
            if (loadingSlider) loadingSlider.value = progressValue;
            isLoading = false;
            yield return null;
        }
    }
}
