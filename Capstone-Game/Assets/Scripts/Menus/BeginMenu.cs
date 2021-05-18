using UnityEngine;
using UnityEngine.SceneManagement;

public class BeginMenu : MonoBehaviour
{
    public Animation anim;

    public void NewGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Resume()
    {
        NewGame();
    }

    public void Back()
    {
        anim.Play("BeginToMainMenu");
    }
}
