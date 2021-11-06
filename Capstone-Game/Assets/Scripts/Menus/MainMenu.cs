using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    public Animation anim;
    public GameObject quitConfirmation;
    public GameObject beginButton;

    private void Start()
    {
        FindObjectOfType<EventSystem>().SetSelectedGameObject(beginButton);
    }

    private void Update()
    {
        if (MenuManager.ButtonsEnabled() && Input.GetButtonDown("Pause"))
        {
            if (quitConfirmation.activeInHierarchy)
            {
                quitConfirmation.SetActive(false);
            }
            else
            {
                Quit();
            }
        }
    }

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
        quitConfirmation.SetActive(true);
    }

    public void ActuallyQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
