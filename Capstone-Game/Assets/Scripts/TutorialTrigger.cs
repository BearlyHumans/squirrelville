using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialTrigger : MonoBehaviour
{
    public GameObject tutorialBox;
    public TMP_Text textObject;
    public TMP_Text hintObject;
    private Animator animator;

    [TextArea]
    public string text;
    [TextArea]
    public string hint;

    private static List<TutorialTrigger> tutorialTriggers = new List<TutorialTrigger>();

    private void Start()
    {
        animator = tutorialBox.GetComponent<Animator>();
        animator.SetBool("visible", false);

        tutorialBox.SetActive(false);

        textObject.text = text;
        hintObject.text = hint;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (tutorialTriggers.IndexOf(this) == -1)
        {
            tutorialBox.SetActive(true);
            animator.SetBool("visible", true);
            tutorialTriggers.Add(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (tutorialTriggers.IndexOf(this) > -1)
        {
            animator.SetBool("visible", false);
            tutorialTriggers.Remove(this);
        }
    }

    public bool IsVisible()
    {
        return animator.GetBool("visible");
    }
}
