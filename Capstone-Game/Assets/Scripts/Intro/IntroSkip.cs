using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class IntroSkip : MonoBehaviour
{
    private void Start()
    {
        GetComponent<VideoPlayer>().loopPointReached += VideoEnded;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            NextScene();
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
