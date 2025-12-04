using System;
using System.Collections;
using UnityEngine;

public class CinematicBar : MonoBehaviour
{
    public GameObject topBar;
    public GameObject bottomBar;

    public float barSpeed = 2.0f;   // Units of screen-space per second (0–1)
    public float topBarScreenY = 0.9f;
    public float bottomBarScreenY = 0.1f;
    public float offScreenOffset = 0.15f;

    private bool isAnimating = false;
    private Camera mainCamera;

    public event Action OnBarsShown;
    public event Action OnBarsHidden;

    void Start()
    {
        if (topBar == null || bottomBar == null)
        {
            Debug.LogError("CinematicBar: Bars not assigned!", gameObject);
            enabled = false;
            return;
        }

        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        // Debug keys
        if (Input.GetKeyDown(KeyCode.O))
            HideBars();

        if (Input.GetKeyDown(KeyCode.P))
            ShowBars();
    }

    public bool IsAnimating => isAnimating;

    public void ShowBars()
    {
        if (isAnimating)
            StopAllCoroutines();

        StartCoroutine(AnimateBars(
            topBarScreenY,
            bottomBarScreenY,
            OnBarsShown
        ));
    }

    public void HideBars()
    {
        if (isAnimating)
            StopAllCoroutines();

        StartCoroutine(AnimateBars(
            1f + offScreenOffset,
            -offScreenOffset,
            OnBarsHidden
        ));
    }

    private Vector3 GetScreenSpaceWorldPosition(float screenX, float screenY)
    {
        // Convert normalized screen position (0–1) to pixel coords
        Vector3 screenPos = new Vector3(
            Screen.width * screenX,
            Screen.height * screenY,
            Mathf.Abs(mainCamera.transform.position.z) + 10f
        );

        return mainCamera.ScreenToWorldPoint(screenPos);
    }

    private float GetBarScreenY(GameObject bar)
    {
        Vector3 barWorldPos = bar.transform.position;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(barWorldPos);
        return screenPos.y / Screen.height;
    }

    private IEnumerator AnimateBars(float targetTopY, float targetBottomY, Action onComplete)
    {
        isAnimating = true;

        float startTopY = GetBarScreenY(topBar);
        float startBottomY = GetBarScreenY(bottomBar);

        float distance = Mathf.Max(
            Mathf.Abs(targetTopY - startTopY),
            Mathf.Abs(targetBottomY - startBottomY)
        );

        float duration = distance / barSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float currentTopY = Mathf.Lerp(startTopY, targetTopY, t);
            float currentBottomY = Mathf.Lerp(startBottomY, targetBottomY, t);

            topBar.transform.position = GetScreenSpaceWorldPosition(0.5f, currentTopY);
            bottomBar.transform.position = GetScreenSpaceWorldPosition(0.5f, currentBottomY);

            yield return null;
        }

        // Snap to final positions
        topBar.transform.position = GetScreenSpaceWorldPosition(0.5f, targetTopY);
        bottomBar.transform.position = GetScreenSpaceWorldPosition(0.5f, targetBottomY);

        isAnimating = false;
        onComplete?.Invoke();
    }
}