using UnityEngine;

public class ControlsMenu : MonoBehaviour
{
    public Animation anim;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Back();
        }
    }

    public void Back()
    {
        anim.Play("ControlsToMainMenu");
    }
}
