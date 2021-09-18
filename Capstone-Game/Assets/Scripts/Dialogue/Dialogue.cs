using UnityEngine;
using UnityEngine.Events;

public class Dialogue : MonoBehaviour
{
    [Tooltip("A list of dialogue entries")]
    public DialogueEntry[] entries;

    [Tooltip("Event evoked when the dialogue starts")]
    public UnityEvent dialogueStart;

    [Tooltip("Event evoked when the player moves onto the next sentence")]
    public UnityEvent dialogueNext;

    [Tooltip("Event evoked when the dialogue finishes")]
    public UnityEvent dialogueFinish;
}
