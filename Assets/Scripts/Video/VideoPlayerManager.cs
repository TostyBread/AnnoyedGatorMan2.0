using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;


public class VideoPlayerManager : MonoBehaviour
{
    private VideoPlayer videoPlayer;

    [Header("Video Player Settings")]
    public float adjustedPlaybackSpeed = 2f;
    private float originalPlaybackSpeed = 1f;

    public string sceneToLoad = "MainMenu";

    // Start is called before the first frame update
    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();

        videoPlayer.loopPointReached += OnVideoEnd;
    }

    // Update is called once per frame
    void Update()
    {
        if (videoPlayer == null) return;

        if (videoPlayer.isPlaying)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                videoPlayer.playbackSpeed = adjustedPlaybackSpeed;
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                videoPlayer.playbackSpeed = originalPlaybackSpeed;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LoadNextScene();
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        LoadNextScene();
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
