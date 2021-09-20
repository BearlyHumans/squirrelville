using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MusicTrigger : MonoBehaviour
{
    [Tooltip("The audio mixer group to play the music in")]
    public AudioMixerGroup group;

    [Tooltip("The music audio clip to play while within the trigger")]
    public AudioClip clip;

    [Tooltip("How loud the music is")]
    [Range(0, 1)]
    public float volume = 1.0f;

    [Tooltip("How many seconds for the music to fade in and out when entering and exiting the trigger")]
    [Min(0)]
    public float fadeTime;

    private AudioSource audioSource;
    private Coroutine fadeCoroutine;

    private static List<MusicTrigger> musicTriggers = new List<MusicTrigger>();

    private void AddAudioSource()
    {
        GameObject obj = new GameObject();

        audioSource = obj.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = group;
        audioSource.clip = clip;
        audioSource.name = clip.name;
        audioSource.loop = true;

        fadeCoroutine = StartCoroutine(FadeIn());
    }

    public void RemoveAudioSource()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        StartCoroutine(FadeOut());
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

    private IEnumerator FadeIn()
    {
        audioSource.volume = 0.0f;
        audioSource.Play();

        while (audioSource.volume < volume)
        {
            audioSource.volume += (volume / fadeTime) * Time.deltaTime;
            audioSource.volume = Mathf.Min(audioSource.volume, volume);
            yield return 0;
        }
    }

    private IEnumerator FadeOut()
    {
        AudioSource audioSource = this.audioSource;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= (volume / fadeTime) * Time.deltaTime;
            audioSource.volume = Mathf.Min(audioSource.volume, volume);
            yield return 0;
        }

        GameObject.Destroy(audioSource.gameObject);
    }
}
