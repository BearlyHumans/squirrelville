using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue")]
public class Dialogue : ScriptableObject
{
    [Tooltip("If set, this is the set of sounds to randomly pick from for every letter spoken in all dialogue entries")]
    public AudioClipSet audioClipSet;

    [Tooltip("A list of dialogue entries")]
    public DialogueEntry[] entries;
}
