using UnityEngine;

[CreateAssetMenu(fileName = "New Audio Clip Set", menuName = "Audio Clip Set")]
public class AudioClipSet : ScriptableObject
{
    public AudioClip[] audioClips;

    public AudioClip GetRandomAudioClip()
    {
        return audioClips[Random.Range(0, audioClips.Length - 1)];
    }
}
