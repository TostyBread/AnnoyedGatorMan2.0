using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

public class AnalyticManager : MonoBehaviour
{
    public static AnalyticManager Instance;
    private bool isInitialized = false;

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

    public void RecordGameoverData(string currentlevel, bool win, int currentscore)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Analytics Service is not initialized yet.");
            return;
        }

        // Create event data payload
        CustomEvent myEvent = new CustomEvent("GameOver")
        {
            { "Level", currentlevel},
            { "Win", win},
            { "Score", currentscore}
        };

        // Send event
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();

        Debug.Log($"[Analytics] GameOver event sent: Level = {currentlevel}, Win = {win}, Score = {currentscore}");
    }
}
