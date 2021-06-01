using UnityEngine;

public class ControlsMenu : MonoBehaviour
{
    public Animation anim;

    public void Back()
    {
        anim.Play("ControlsToMainMenu");
    }
}
