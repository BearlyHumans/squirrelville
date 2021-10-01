using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue")]
public class Dialogue : ScriptableObject
{
    [Tooltip("A list of dialogue entries")]
    public DialogueEntry[] entries;
}
