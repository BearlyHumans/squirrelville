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

    public void Controls()
    {
        anim.Play("MainToControlsMenu");
    }

    public void Credits()
    {
        anim.Play("MainToCreditsMenu");
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
