using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MusicTrigger : MonoBehaviour
{
    public AudioMixerGroup group;
    public AudioClip clip;
    [Range(0, 1)]
    public float volume = 1.0f;

    private GameObject obj;

    private static List<MusicTrigger> musicTriggers = new List<MusicTrigger>();

    private void AddAudioSource()
    {
        obj = new GameObject();

        AudioSource audioSource = obj.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = group;
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.name = clip.name;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void RemoveAudioSource()
    {
        GameObject.Destroy(obj);
        obj = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other) && musicTriggers.IndexOf(this) == -1)
        {
            if (musicTriggers.Count > 0)
            {
                musicTriggers[musicTriggers.Count - 1].RemoveAudioSource();
            }

            AddAudioSource();
            musicTriggers.Add(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsPlayer(other) && musicTriggers.IndexOf(this) > -1)
        {
            if (musicTriggers.Count > 1 && musicTriggers[musicTriggers.Count - 1] == this)
            {
                musicTriggers[musicTriggers.Count - 2].AddAudioSource();
            }

            RemoveAudioSource();
            musicTriggers.Remove(this);
        }
    }

    private bool IsPlayer(Collider collider)
    {
        return collider.gameObject.layer == LayerMask.NameToLayer("Player");
    }
}
