using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    private Button[] buttons;
    private static bool buttonsEnabled = true;

    private void Awake()
    {
        buttons = GetComponentsInChildren<Button>(true);
    }

    private void DisableButtons()
    {
        foreach (Button button in buttons)
        {
            button.interactable = false;
        }

        buttonsEnabled = false;
    }

    private void EnableButtons()
    {
        foreach (Button button in buttons)
        {
            button.interactable = true;
        }

        buttonsEnabled = true;
    }

    public static bool ButtonsEnabled()
    {
        return buttonsEnabled;
    }
}
