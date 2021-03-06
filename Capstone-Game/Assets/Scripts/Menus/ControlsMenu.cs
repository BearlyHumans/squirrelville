using UnityEngine;
using UnityEngine.UI;

public class ControlsMenu : MonoBehaviour
{
    public Animation anim;
    public Button backButton;

    private void Update()
    {
        if (MenuManager.ButtonsEnabled() && Input.GetButton("Pause"))
        {
            backButton.onClick.Invoke();
        }
    }

    public void Back()
    {
        anim.Play("ControlsToMainMenu");
    }
}
