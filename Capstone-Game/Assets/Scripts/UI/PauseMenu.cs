using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    /// <summary> Whether the game is currently paused </summary>
    public static bool paused = false;
    public GameObject pauseMenu;
    public GameObject mainMenu;
    public GameObject quitConfirmationPrompt;

    private void Start()
    {
        Resume();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Pause"))
        {
            if (paused)
            {
                if (quitConfirmationPrompt.activeInHierarchy)
                {
                    quitConfirmationPrompt.SetActive(false);
                }
                else if (!mainMenu.activeInHierarchy)
                {
                    Resume();
                }
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        paused = false;
    }

    public void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        paused = true;
    }

    public void MainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
}
