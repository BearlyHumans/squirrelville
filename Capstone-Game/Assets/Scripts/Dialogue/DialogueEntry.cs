using UnityEngine;

[System.Serializable]
public class DialogueEntry
{
    [Tooltip("The text to display in the dialogue box")]
    [TextArea]
    public string text;

    [Tooltip("The character sprite to display above the dialogue box")]
    public Sprite sprite;

    [Tooltip("The set of sounds to randomly pick from for every letter spoken in this dialogue")]
    public AudioClipSet audioClipSet;
}
