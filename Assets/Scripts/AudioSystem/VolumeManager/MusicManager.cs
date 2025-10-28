using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [System.Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public AudioClip musicClip;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0f, 5f)]
        public float fadeTime = 1f;
    }

    [Header("Music Settings")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private SceneMusic[] sceneMusicMap;

    [Header("Default (fallback) Music")]
    [SerializeField] private AudioClip defaultMusicClip;
    [Range(0f, 1f)]
    [SerializeField] private float defaultVolume = 1f;
    [Range(0f, 5f)]
    [SerializeField] private float defaultFadeTime = 1f;

    [Header("Special music behavior")]
    [Tooltip("If true, loading a new scene will cancel any active special music and let scene music take over.")]
    [SerializeField] private bool allowSceneToInterruptSpecial = true;

    private AudioClip currentMusic;
    private Coroutine fadeCoroutine;
    private float currentSceneVolume = 0.5f; // per-scene configured volume (0..1)

    // Special-music state
    private Coroutine specialCoroutine;
    private AudioClip savedPrevClip;
    private float savedPrevTime;
    private float savedPrevSceneVolume;
    private bool savedPrevLoop;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Create AudioSource if not assigned
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.loop = true;
            }

            // Subscribe to scene loading events
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Ensure we handle the initially loaded scene
        PlaySceneMusic(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        // Keep music volume in sync with BackgroundVolumeManager.CurrentVolume while playing
        // but do not override while a fade coroutine is running
        if (musicSource != null && musicSource.isPlaying && fadeCoroutine == null && specialCoroutine == null)
        {
            musicSource.volume = currentSceneVolume * BackgroundVolumeManager.CurrentVolume;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If a special is active, optionally let the scene interrupt it.
        if (specialCoroutine != null)
        {
            if (allowSceneToInterruptSpecial)
            {
                // Cancel special music coroutine so scene-driven music can take over.
                StopCoroutine(specialCoroutine);
                specialCoroutine = null;

                // Also stop any special-related fade to avoid coroutine conflicts.
                if (fadeCoroutine != null)
                {
                    StopCoroutine(fadeCoroutine);
                    fadeCoroutine = null;
                }

                // Clear saved previous state — we intentionally discard previous special state
                // because scene-change should make the scene music authoritative.
                savedPrevClip = null;
            }
            else
            {
                // Keep special music active; ignore scene-driven changes.
                return;
            }
        }

        PlaySceneMusic(scene.name);
    }

    public void PlaySceneMusic(string sceneName)
    {
        // If a special music is active, ignore scene-driven changes until special completes or is cancelled.
        if (specialCoroutine != null) return;

        SceneMusic sceneMusic = System.Array.Find(sceneMusicMap, music => music.sceneName == sceneName);

        // If a specific mapping exists for this scene, use it.
        if (sceneMusic != null)
        {
            // If same clip is already the current music, do NOT stop/restart it.
            // Instead, update target volume (and optionally fade to new per-scene volume).
            if (sceneMusic.musicClip == currentMusic)
            {
                // Update the per-scene configured volume
                currentSceneVolume = Mathf.Clamp01(sceneMusic.volume);

                // If there's an ongoing fade, stop it and start a volume-only fade to the new target.
                if (fadeCoroutine != null)
                {
                    StopCoroutine(fadeCoroutine);
                }

                float liveTarget = currentSceneVolume * BackgroundVolumeManager.CurrentVolume;

                // If music is not playing (edge case), start it without reloading the clip.
                if (!musicSource.isPlaying)
                {
                    musicSource.Play();
                    musicSource.volume = 0f;
                    fadeCoroutine = StartCoroutine(FadeVolume(0f, liveTarget, sceneMusic.fadeTime));
                }
                else
                {
                    // Smoothly adjust volume to new target without restarting the clip
                    fadeCoroutine = StartCoroutine(FadeVolume(musicSource.volume, liveTarget, sceneMusic.fadeTime));
                }

                return;
            }

            // Different clip requested -> crossfade
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            currentSceneVolume = Mathf.Clamp01(sceneMusic.volume);
            fadeCoroutine = StartCoroutine(CrossFadeMusic(sceneMusic.musicClip, sceneMusic.fadeTime));
            return;
        }

        // No mapping for this scene:
        // - If we already have music playing, keep it (do not fade out / change).
        // - If nothing is playing and a default is provided, start the default music.
        if (currentMusic != null && musicSource.isPlaying)
        {
            // Keep playing current music; but update volume target if the current clip is the default and per-default volume is different.
            if (currentMusic == defaultMusicClip)
            {
                // Ensure currentSceneVolume reflects default
                currentSceneVolume = Mathf.Clamp01(defaultVolume);
                if (fadeCoroutine != null)
                {
                    StopCoroutine(fadeCoroutine);
                }
                float liveTarget = currentSceneVolume * BackgroundVolumeManager.CurrentVolume;
                fadeCoroutine = StartCoroutine(FadeVolume(musicSource.volume, liveTarget, defaultFadeTime));
            }
            return;
        }

        // Nothing playing -> try to start default music if available
        if (defaultMusicClip != null)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            currentSceneVolume = Mathf.Clamp01(defaultVolume);

            // If the default is already the currentMusic but not playing, start it and fade in
            if (defaultMusicClip == currentMusic)
            {
                float liveTarget = currentSceneVolume * BackgroundVolumeManager.CurrentVolume;
                musicSource.Play();
                musicSource.volume = 0f;
                fadeCoroutine = StartCoroutine(FadeVolume(0f, liveTarget, defaultFadeTime));
            }
            else
            {
                // Crossfade from whatever state to default
                fadeCoroutine = StartCoroutine(CrossFadeMusic(defaultMusicClip, defaultFadeTime));
            }
        }
    }

    /// <summary>
    /// Play a special (priority) music clip triggered from outside.
    /// Defaults: play once (loop = false) and restore previous music when finished (returnToPrevious = true).
    /// </summary>
    /// <param name="specialClip">The special clip to play.</param>
    /// <param name="perClipVolume">Per-clip volume (0..1) applied before master BackgroundVolumeManager.</param>
    /// <param name="fadeTime">Fade time used when switching in/out.</param>
    /// <param name="loop">If true, special music will loop until manually stopped.</param>
    /// <param name="returnToPrevious">If true (default), restores previous music after special completes.</param>
    public void PlaySpecialMusic(AudioClip specialClip, float perClipVolume = 1f, float fadeTime = 0.5f, bool loop = false, bool returnToPrevious = true)
    {
        if (specialClip == null) return;

        // If another special is active, stop it and optionally restore previous immediately.
        if (specialCoroutine != null)
        {
            StopCoroutine(specialCoroutine);
            specialCoroutine = null;
            // Do not auto-restore here; we will proceed to start the new special and the previous state is already saved.
        }

        specialCoroutine = StartCoroutine(SpecialMusicRoutine(specialClip, Mathf.Clamp01(perClipVolume), Mathf.Max(0.0001f, fadeTime), loop, returnToPrevious));
    }

    /// <summary>
    /// Stop a currently playing special music and optionally return to previous music immediately.
    /// </summary>
    public void StopSpecialMusic(bool returnToPrevious = true, float fadeTime = 0.5f)
    {
        if (specialCoroutine != null)
        {
            StopCoroutine(specialCoroutine);
            specialCoroutine = null;

            if (returnToPrevious)
            {
                // Start restoring previous state
                if (savedPrevClip != null)
                {
                    if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                    fadeCoroutine = StartCoroutine(RestorePreviousMusic(fadeTime));
                }
            }
            else
            {
                // Stop and clear current music
                musicSource.Stop();
                currentMusic = null;
            }
        }
    }

    private IEnumerator SpecialMusicRoutine(AudioClip specialClip, float perClipVolume, float fadeTime, bool loop, bool returnToPrevious)
    {
        // Save previous state
        savedPrevClip = musicSource.clip;
        savedPrevTime = musicSource.isPlaying ? musicSource.time : 0f;
        savedPrevSceneVolume = currentSceneVolume;
        savedPrevLoop = musicSource.loop;

        // Make sure previous state is remembered as currentMusic
        // (currentMusic may already equal savedPrevClip)
        // Start crossfade to special
        currentSceneVolume = perClipVolume;
        musicSource.loop = loop;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(CrossFadeMusic(specialClip, fadeTime));

        // Wait until crossfade finishes
        while (fadeCoroutine != null) yield return null;

        // If looping, do nothing more until manually stopped
        if (loop)
        {
            // leave specialCoroutine active so StopSpecialMusic can cancel
            specialCoroutine = null;
            yield break;
        }

        // Play once: wait for clip duration minus small epsilon to account for timing
        float waitTime = (specialClip != null) ? specialClip.length : 0f;
        // If audio system or a user changed volume mid-play, that doesn't affect waiting.
        float elapsed = 0f;
        while (elapsed < waitTime)
        {
            // If special was cancelled externally, stop routine
            if (specialCoroutine == null) yield break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Completed special play
        specialCoroutine = null;

        if (returnToPrevious && savedPrevClip != null)
        {
            // Restore previous music and state
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(RestorePreviousMusic(fadeTime));
        }
        else
        {
            // Do not restore previous. Stop if not looping.
            musicSource.loop = savedPrevLoop; // reset loop to previous preference
            currentMusic = musicSource.clip; // it's still specialClip
            if (!musicSource.loop)
            {
                musicSource.Stop();
                currentMusic = null;
            }
        }
    }

    private IEnumerator RestorePreviousMusic(float fadeTime)
    {
        // Fade out current (special) quickly
        float startVol = musicSource.volume;
        float halfFade = Mathf.Max(0.0001f, fadeTime * 0.5f);
        float elapsed = 0f;
        while (elapsed < halfFade)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVol, 0f, elapsed / halfFade);
            yield return null;
        }

        // Switch back to previous clip and resume from saved time
        musicSource.clip = savedPrevClip;
        currentMusic = savedPrevClip;
        musicSource.time = Mathf.Clamp(savedPrevTime, 0f, savedPrevClip != null ? savedPrevClip.length : 0f);
        musicSource.loop = savedPrevLoop;
        musicSource.Play();

        // Restore per-scene volume from savedPrevSceneVolume
        currentSceneVolume = savedPrevSceneVolume;

        // Fade in to live target (respects BackgroundVolumeManager)
        float target = currentSceneVolume * BackgroundVolumeManager.CurrentVolume;
        elapsed = 0f;
        while (elapsed < halfFade)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, target, elapsed / halfFade);
            yield return null;
        }

        musicSource.volume = target;
        fadeCoroutine = null;
    }

    private IEnumerator CrossFadeMusic(AudioClip newClip, float fadeTime)
    {
        float startVol = musicSource.volume;

        // Fade out current music if playing
        if (musicSource.isPlaying)
        {
            float elapsed = 0;
            float halfFade = Mathf.Max(0.0001f, fadeTime * 0.5f);
            while (elapsed < halfFade)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVol, 0f, elapsed / halfFade);
                yield return null;
            }
        }

        // Switch to new clip
        musicSource.clip = newClip;
        musicSource.Play();
        currentMusic = newClip;

        // Fade in new music (respect live BackgroundVolumeManager value)
        float elapsed2 = 0;
        float halfFadeIn = Mathf.Max(0.0001f, fadeTime * 0.5f);
        while (elapsed2 < halfFadeIn)
        {
            elapsed2 += Time.deltaTime;
            float liveTarget = currentSceneVolume * BackgroundVolumeManager.CurrentVolume;
            musicSource.volume = Mathf.Lerp(0f, liveTarget, elapsed2 / halfFadeIn);
            yield return null;
        }

        musicSource.volume = currentSceneVolume * BackgroundVolumeManager.CurrentVolume;
        fadeCoroutine = null;
    }

    private IEnumerator FadeVolume(float from, float to, float duration)
    {
        float elapsed = 0f;
        float safeDuration = Mathf.Max(0.0001f, duration);
        while (elapsed < safeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / safeDuration;
            float intended = Mathf.Lerp(from, to, t);
            musicSource.volume = intended;
            yield return null;
        }

        // Ensure final volume respects live BackgroundVolumeManager value
        musicSource.volume = currentSceneVolume * BackgroundVolumeManager.CurrentVolume;
        fadeCoroutine = null;
    }

    // Keep this method if other code expects to call it; it will apply the BackgroundVolumeManager's current value.
    public void SetMasterVolume(float volume)
    {
        // Do not store a separate master volume here.
        // Apply BackgroundVolumeManager.CurrentVolume (BackgroundVolumeManager owns the source of truth).
        if (musicSource.isPlaying)
        {
            musicSource.volume = currentSceneVolume * BackgroundVolumeManager.CurrentVolume;
        }
    }
}
