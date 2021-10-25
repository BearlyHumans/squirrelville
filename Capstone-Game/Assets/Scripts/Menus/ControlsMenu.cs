using UnityEngine;

public class ControlsMenu : MonoBehaviour
{
    public Animation anim;

    private void Update()
    {
        if (MenuManager.ButtonsEnabled() && Input.GetKey(KeyCode.Escape))
        {
            Back();
        }
    }

    public void Back()
    {
        anim.Play("ControlsToMainMenu");
    }
}
