using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class MenuManager : MonoBehaviour
{
    private Button[] buttons;
    private static bool buttonsEnabled = true;

    public UnityEvent creditsFinish;

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

    private void FinishCredits()
    {
        creditsFinish?.Invoke();
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
