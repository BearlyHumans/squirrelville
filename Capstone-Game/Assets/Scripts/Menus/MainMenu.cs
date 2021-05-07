using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Animation anim;

    public void Play()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Options()
    {
        anim.Play("MainToOptionsMenu");
    }

    public void Credits()
    {
        anim.Play("MainToCreditsMenu");
    }

    public void Quit()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
