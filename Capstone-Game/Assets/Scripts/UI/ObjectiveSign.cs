using UnityEngine;
using System.Collections;
using TMPro;

[RequireComponent(typeof(Animator))]
public class ObjectiveSign : MonoBehaviour
{
    public TMP_Text text;
    public float visibleSeconds = 5.0f;
    private Coroutine coroutine;
    private Animator animator;
    private bool hasObjective = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("visible", false);
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (hasObjective)
        {
            if (PauseMenu.paused)
            {
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                    coroutine = null;
                }
                animator.SetBool("visible", true);
            }
            else if (coroutine == null)
            {
                animator.SetBool("visible", false);
            }
        }
    }

    public void SetObjective(string objective)
    {
        gameObject.SetActive(true);

        hasObjective = true;
        text.text = objective;

        if (coroutine != null)
            StopCoroutine(coroutine);
        coroutine = StartCoroutine(ShowBriefly());
    }

    public void RemoveObjective()
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
        animator.SetBool("visible", false);
        hasObjective = false;
    }

    private IEnumerator ShowBriefly()
    {
        animator.SetBool("visible", true);
        yield return new WaitForSeconds(visibleSeconds);
        animator.SetBool("visible", false);
    }

    public bool IsVisible()
    {
        return animator.GetBool("visible");
    }

    public bool HasObjective()
    {
        return hasObjective;
    }
}
