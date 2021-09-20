using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioTrigger : MonoBehaviour
{
    public List<AdvancedAudioClip> audioClips;
    private List<GameObject> gameObjects = new List<GameObject>();

    private static List<AudioTrigger> audioTriggers = new List<AudioTrigger>();

    private void AddAudioSources()
    {
        foreach (AdvancedAudioClip audioClip in audioClips)
        {
            GameObject gameObject = new GameObject();
            gameObjects.Add(gameObject);

            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = audioClip.clip;
            audioSource.outputAudioMixerGroup = audioClip.group;
            audioSource.volume = audioClip.volume;
            audioSource.name = audioClip.clip.name;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void RemoveAudioSources()
    {
        foreach (GameObject gameObject in gameObjects)
        {
            GameObject.Destroy(gameObject);
        }

        gameObjects.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other) && audioTriggers.IndexOf(this) == -1)
        {
            if (audioTriggers.Count > 0)
            {
                audioTriggers[audioTriggers.Count - 1].RemoveAudioSources();
            }

            AddAudioSources();
            audioTriggers.Add(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsPlayer(other) && audioTriggers.IndexOf(this) > -1)
        {
            if (audioTriggers.Count > 1 && audioTriggers[audioTriggers.Count - 1] == this)
            {
                audioTriggers[audioTriggers.Count - 2].AddAudioSources();
            }

            RemoveAudioSources();
            audioTriggers.Remove(this);
        }
    }

    private bool IsPlayer(Collider collider)
    {
        return collider.gameObject.layer == LayerMask.NameToLayer("Player");
    }
}

[System.Serializable]
public class AdvancedAudioClip
{
    public AudioClip clip;
    public AudioMixerGroup group;
    [Range(0, 1)]
    public float volume = 1.0f;
}
