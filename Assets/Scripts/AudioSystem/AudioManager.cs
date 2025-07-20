using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Settings")]
    public AudioSource audioSourcePrefab; // Prefab for creating AudioSources
    public int poolSize = 10; // Number of reusable AudioSources

    public float AdjustedVolume = 1.0f; // Default volume for audio sources, need to be public so SfxVolumeManager can change it

    private Queue<AudioSource> audioPool = new Queue<AudioSource>();
    private Dictionary<string, List<AudioClip>> audioClips = new Dictionary<string, List<AudioClip>>();
    private List<AudioSource> activeSources = new List<AudioSource>(); // Track active sources

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioPool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // In case if a pooled AudioSource GameObject is disabled externally (e.g. scene unload),
    // it may leave stale references in activeSources. So just a precaution for memory leak
    private void OnDisable()
    {
        for (int i = activeSources.Count - 1; i >= 0; i--)
        {
            if (!activeSources[i].isPlaying)
            {
                activeSources[i].gameObject.SetActive(false);
                audioPool.Enqueue(activeSources[i]);
                activeSources.RemoveAt(i);
            }
        }
    }

    private void InitializeAudioPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource newSource = Instantiate(audioSourcePrefab, transform);
            newSource.gameObject.SetActive(false);
            audioPool.Enqueue(newSource);
        }
    }

    // GUIDE ON HOW TO USE RANDOM VARIABLE SOUND:
    // you must name your audio file in "Example_1" as in name followed by underscore before a number
    // like "Example_1" "Example_2" will be recognize and stored as "Example"
    // and when you want to instantiate it, you need to put the following code:
    // AudioManager.Instance.PlaySound("Example"); or AudioManager.Instance.PlaySound("Example", transform.position);
    // not "Example_1", the system will not recognize it
    // IF THERE IS NO VARIATION, DO NOT USE UNDERSCORE

    public void PlaySound(string soundName, Vector3? position = null)
    {
        if (!audioClips.TryGetValue(soundName, out List<AudioClip> clips) || clips.Count == 0)
        {
            Debug.LogWarning("AudioManager: Sound " + soundName + " not found!");
            return;
        }

        AudioClip clip = clips[Random.Range(0, clips.Count)]; // Pick a random variation
        AudioSource source = GetAvailableAudioSource();
        if (source != null)
        {
            source.clip = clip;
            source.volume = AdjustedVolume;
            source.gameObject.SetActive(true);
            source.transform.position = position ?? Vector3.zero;
            source.Play();
            activeSources.Add(source); // Track active source
            StartCoroutine(ReturnAudioSourceToPool(source, clip.length));
        }
    }

    //Comenting out the PlaySound method as it's volume is not adjustable by SfxVolumeManager
    //public void PlaySound(string soundName, float volume = 1, Vector3? position = null)
    //{
    //    if (!audioClips.TryGetValue(soundName, out List<AudioClip> clips) || clips.Count == 0)
    //    {
    //        Debug.LogWarning("AudioManager: Sound " + soundName + " not found!");
    //        return;
    //    }

    //    AudioClip clip = clips[Random.Range(0, clips.Count)]; // Pick a random variation
    //    AudioSource source = GetAvailableAudioSource();
    //    if (source != null)
    //    {
    //        source.clip = clip;
    //        source.volume = volume;
    //        source.gameObject.SetActive(true);
    //        source.transform.position = position ?? Vector3.zero;
    //        source.Play();
    //        activeSources.Add(source); // Track active source
    //        StartCoroutine(ReturnAudioSourceToPool(source, clip.length));
    //    }
    //}

    public void StopSound(string soundName)
    {
        for (int i = activeSources.Count - 1; i >= 0; i--)
        {
            AudioSource source = activeSources[i];
            if (source.isPlaying && source.clip != null && audioClips.ContainsKey(soundName) && audioClips[soundName].Contains(source.clip))
            {
                source.Stop();
                source.gameObject.SetActive(false);
                audioPool.Enqueue(source);
                activeSources.RemoveAt(i);
            }
        }
    }

    private AudioSource GetAvailableAudioSource()
    {
        if (audioPool.Count > 0)
        {
            return audioPool.Dequeue();
        }

        AudioSource newSource = Instantiate(audioSourcePrefab, transform);
        return newSource;
    }

    private System.Collections.IEnumerator ReturnAudioSourceToPool(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        source.Stop();
        source.gameObject.SetActive(false);
        audioPool.Enqueue(source);
        activeSources.Remove(source);
    }

    public void RegisterSound(string soundName, AudioClip clip)
    {
        if (!audioClips.ContainsKey(soundName))
        {
            audioClips[soundName] = new List<AudioClip>();
        }
        audioClips[soundName].Add(clip);
    }

    public void RegisterSounds(List<AudioClip> clips)
    {
        foreach (AudioClip clip in clips)
        {
            string baseName = clip.name.Contains("_") ? clip.name.Split('_')[0] : clip.name;
            RegisterSound(baseName, clip); // Group if it has an underscore, otherwise use full name
        }
    }
}
