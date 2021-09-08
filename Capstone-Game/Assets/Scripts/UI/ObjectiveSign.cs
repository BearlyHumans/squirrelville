using UnityEngine;
using System.Collections;
using TMPro;

public class ObjectiveSign : MonoBehaviour
{
    public TMP_Text text;
    private bool visible = false;
    private Coroutine coroutine;

    private void Start()
    {
        text.text = "";
        visible = false;
    }

    public void SetObjective(string objective)
    {
        text.text = objective;
        visible = true;

        if (coroutine != null)
            StopCoroutine(coroutine);
        coroutine = StartCoroutine(ShowBriefly());
    }

    private IEnumerator ShowBriefly()
    {
        yield return new WaitForSeconds(5);

        text.text = "";
        visible = false;
    }

    public bool isVisible()
    {
        return PauseMenu.paused || visible;
    }
}
