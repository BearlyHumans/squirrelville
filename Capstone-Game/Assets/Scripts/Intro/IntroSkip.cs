using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class IntroSkip : MonoBehaviour
{
    private VideoPlayer videoPlayer;

    private void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.loopPointReached += VideoEnded;
        Cursor.visible = false;
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

            if (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Escape))
            {
                NextScene();
            }
        }
    }

    private void VideoEnded(VideoPlayer videoPlayer)
    {
        NextScene();
    }

    private void NextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
