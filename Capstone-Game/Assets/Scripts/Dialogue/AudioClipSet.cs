using UnityEngine;

[CreateAssetMenu(fileName = "New Audio Clip Set", menuName = "Audio Clip Set")]
public class AudioClipSet : ScriptableObject
{
    public AudioClip[] audioClips;

    [Tooltip("The minimum random pitch of the audio clips")]
    public float minimumPitch = 1.0f;

    [Tooltip("The maximum random pitch of the audio clips")]
    public float maximumPitch = 1.0f;

    public AudioClip GetRandomAudioClip()
    {
        return audioClips[Random.Range(0, audioClips.Length - 1)];
    }

    public float getRandomPitch()
    {
        return Random.Range(minimumPitch, maximumPitch);
    }
}
