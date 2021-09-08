using UnityEngine;
using System.Collections;
using TMPro;

[RequireComponent(typeof(Animation))]
public class ObjectiveSign : MonoBehaviour
{
    public TMP_Text text;
    private bool visible = false;
    private Coroutine coroutine;
    private Animation anim;

    private void Start()
    {
        anim = GetComponent<Animation>();
        gameObject.SetActive(false);

        text.text = "";
        visible = false;
    }

    public void SetObjective(string objective)
    {
        gameObject.SetActive(true);
        text.text = objective;

        if (coroutine != null)
            StopCoroutine(coroutine);
        coroutine = StartCoroutine(ShowBriefly());
    }

    private IEnumerator ShowBriefly()
    {
        anim.Play("Enter");
        yield return new WaitForSeconds(5);
        anim.Play("Exit");
    }
}
