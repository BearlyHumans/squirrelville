using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioTrigger : MonoBehaviour
{
    public List<AdvancedAudioClip> audioClips;
    private List<GameObject> gameObjects = new List<GameObject>();

    private void AddAudioSources()
    {
        if (gameObjects.Count > 0) return;

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

    private void RemoveAudioSources()
    {
        if (gameObjects.Count == 0) return;

        foreach (GameObject gameObject in gameObjects)
        {
            GameObject.Destroy(gameObject);
        }

        gameObjects.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other))
        {
            AddAudioSources();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsPlayer(other))
        {
            RemoveAudioSources();
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
