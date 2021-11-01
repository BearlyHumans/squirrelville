using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class IntroSkip : MonoBehaviour
{
    [Tooltip("The skip text element")]
    public HideUIElementAfterDelay skipText;

    private VideoPlayer videoPlayer;
    private AsyncOperation asyncLoad;

    private void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.loopPointReached += VideoEnded;
        Cursor.visible = false;

        asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        asyncLoad.allowSceneActivation = false;
    }

    private void Update()
    {
        if (PauseMenu.paused)
        {
            if (videoPlayer.isPlaying)
                videoPlayer.Pause();
        }
        else
        {
            if (videoPlayer.isPaused)
                videoPlayer.Play();

            if (Input.GetButtonDown("SkipIntro"))
            {
                NextScene();
            }
            else if (Input.anyKeyDown && !Input.GetButtonDown("Pause"))
            {
                skipText.Show();
            }
        }
    }

    private void VideoEnded(VideoPlayer videoPlayer)
    {
        NextScene();
    }

    private void NextScene()
    {
        asyncLoad.allowSceneActivation = true;
    }
}
