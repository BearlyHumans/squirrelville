using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class TutorialTrigger : MonoBehaviour
{
    private static List<TutorialTrigger> tutorialTriggers = new List<TutorialTrigger>();

    private void Start()
    {
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
            print("Enter " + name);
            tutorialTriggers.Add(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (tutorialTriggers.IndexOf(this) > -1)
        {
            print("Exit " + name);
            tutorialTriggers.Remove(this);
        }
    }
}
