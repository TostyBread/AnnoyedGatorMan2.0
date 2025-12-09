using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AnalyticManager : MonoBehaviour
{
    public static AnalyticManager Instance;

    private bool isInitialized = false;

    // Data tracking fields
    private int fireCount = 0;
    private int pestCount = 0;
    private string pestType;
    private string currentWeather;
    private Vector2 pestSpawnPos;
    private string pestSpawner;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    async void Start()
    {
        await UnityServices.InitializeAsync();
        AnalyticsService.Instance.StartDataCollection();
        isInitialized = true;
    }

    public void RecordGameoverData(string currentLevel, bool win, int currentScore, int playedTime)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Analytics Service is not initialized yet.");
            return;
        }

        // Create event data payload
        CustomEvent myEvent = new CustomEvent("GameOver")
        {
            { "Level", currentLevel},
            { "Win", win},
            { "Score", currentScore},
            { "Playtime", playedTime}
        };

        // Send event
        AnalyticsService.Instance.RecordEvent(myEvent);

        Debug.Log($"[Analytics] GameOver event sent: Level = {currentLevel}, Win = {win}, Score = {currentScore}, PlayedTime = {playedTime}");
    }

    public void RecordServeOrderData(string orderId, int serveTime)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Analytics Service is not initialized yet.");
            return;
        }

        // Create event data payload
        CustomEvent myEvent = new CustomEvent("ServeOrder")
        {
            { "OrderId", orderId},
            { "ServeTime", serveTime},
            { "Total_Intteruption", fireCount + pestCount}
        };

        // Send event
        AnalyticsService.Instance.RecordEvent(myEvent);

        Debug.Log($"[Analytics] ServeOrder event sent: Order = {orderId}, ServedTime = {serveTime}");
    }

    public void TrackFireInterruption(int fire)
    {
        fireCount = fire;
        //Debug.Log($"[Analytics] Fire tracked: {fireCount}");
    }

    public void TrackPestInterruption(int pestNum)
    {
        pestCount = pestNum;
        //Debug.Log($"[Analytics] Pest tracked: {pestCount}");
    }

    public void TrackPestType(string weather, string pestName)
    { 
        if (!isInitialized)
        {
            Debug.LogWarning("Analytics Service is not initialized yet.");
            return;
        }

        // Create event data payload
        CustomEvent myEvent = new CustomEvent("GameOver")
        {
            { "Weather of current level", weather},
            { "Pest name", pestName},
        };

        // Send event
        AnalyticsService.Instance.RecordEvent(myEvent);

        Debug.Log($"[Analytics] GameOver event sent: Level = {weather}, Win = {pestName}");
    }

    public void TrackPestSpawnPos(Transform pos)
    {
        pestSpawner = pos.name;
        pestSpawnPos = new Vector2(pos.position.x, pos.position.y);
    }

    public void TrackTimeSpendOnScreen()
    {
        string screenName = SceneManager.GetActiveScene().name;

        int timeOnScreen = 0;
        timeOnScreen += Mathf.RoundToInt(Time.deltaTime);

        // Create event data payload
        CustomEvent myEvent = new CustomEvent("TimeSpendOnScreen")
        {
            { "ScreenName", screenName},
            { "TimeOnScreen", timeOnScreen}
        };

        // Send event
        AnalyticsService.Instance.RecordEvent(myEvent);

        Debug.Log($"[Analytics] TimeSpendOnScreen event sent: ScreenName = {screenName}, TimeOnScreen = {timeOnScreen}");
    }
}
