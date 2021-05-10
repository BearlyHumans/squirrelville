using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public Animation anim;

    public void Play()
    {
        anim.Play("MainToBeginMenu");
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
