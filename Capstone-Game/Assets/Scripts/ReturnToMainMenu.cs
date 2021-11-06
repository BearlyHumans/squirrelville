using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class ReturnToMainMenu : MonoBehaviour
{
    public float delay = 5.0f;
    public UnityEvent onFadeOut;

    private void OnEnable()
    {
        Invoke("FadeOut", delay);
    }

    private void FadeOut()
    {
        FadeToBlack.singleton.BecomeOpaque(1.0f);
        Invoke("FadedOut", 1.0f);
    }

    private void FadedOut()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        onFadeOut.Invoke();
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MENU");
    }
}
