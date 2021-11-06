using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMainMenu : MonoBehaviour
{
    public float delay = 5.0f;

    private void OnEnable()
    {
        Invoke("FadeOut", delay);
    }

    private void FadeOut()
    {
        FadeToBlack.singleton.BecomeOpaque(1.0f);
        Invoke("GoToMainMenu", 1.0f);
    }

    private void GoToMainMenu()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("MENU");
    }
}
