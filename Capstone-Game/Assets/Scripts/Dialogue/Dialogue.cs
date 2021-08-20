using UnityEngine;
using UnityEngine.Events;

public class Dialogue : MonoBehaviour
{
    [Tooltip("A list of sentences to display as part of the dialogue")]
    [TextArea]
    public string[] sentences;

    [Tooltip("Event evoked when the dialogue starts")]
    public UnityEvent dialogueStart;

    [Tooltip("Event evoked when the player moves onto the next sentence")]
    public UnityEvent dialogueNext;

    [Tooltip("Event evoked when the dialogue finishes")]
    public UnityEvent dialogueFinish;
}
