using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialTrigger : MonoBehaviour
{
    public GameObject tutorialBox;
    public TMP_Text textObject;
    public TMP_Text hintObject;

    [TextArea]
    public string text;
    [TextArea]
    public string hint;

    private static List<TutorialTrigger> tutorialTriggers = new List<TutorialTrigger>();

    private void Start()
    {
        textObject.text = text;
        hintObject.text = hint;
        tutorialBox.SetActive(false);

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            // meshRenderer.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (tutorialTriggers.IndexOf(this) == -1)
        {
            print("Enter " + name);
            tutorialBox.SetActive(true);
            tutorialTriggers.Add(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (tutorialTriggers.IndexOf(this) > -1)
        {
            print("Exit " + name);
            tutorialBox.SetActive(false);
            tutorialTriggers.Remove(this);
        }
    }
}
