using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AmbienceTrigger : MonoBehaviour
{
    [Tooltip("The audio mixer group to play the ambient sounds in")]
    public AudioMixerGroup group;

    public List<AmbienceClip> ambience = new List<AmbienceClip>();

    [Tooltip("How many seconds for the ambient sounds to fade in and out when entering and exiting the trigger")]
    [Min(0)]
    public float fadeTime;

    private List<AudioSource> audioSources = new List<AudioSource>();
    private Coroutine fadeCoroutine;

    private static List<AmbienceTrigger> ambienceTriggers = new List<AmbienceTrigger>();

    private void Start()
    {
        AddAudioSources();
    }

    private void AddAudioSources()
    {
        foreach (AmbienceClip ambienceClip in ambience)
        {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = group;
            audioSource.clip = ambienceClip.clip;
            audioSource.name = ambienceClip.clip.name;
            audioSource.volume = 0.0f;
            audioSource.loop = true;
            audioSource.Play();

            ambienceClip.audioSource = audioSource;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other) && ambienceTriggers.IndexOf(this) == -1)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            fadeCoroutine = StartCoroutine(FadeIn());
            ambienceTriggers.Add(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsPlayer(other) && ambienceTriggers.IndexOf(this) > -1)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            fadeCoroutine = StartCoroutine(FadeOut());
            ambienceTriggers.Remove(this);
        }
    }

    private bool IsPlayer(Collider collider)
    {
        return collider.gameObject.layer == LayerMask.NameToLayer("Player");
    }

    private IEnumerator FadeIn()
    {
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            // Increase the volume of each audio source
            foreach (AmbienceClip ambienceClip in ambience)
            {
                ambienceClip.audioSource.volume += (ambienceClip.volume / fadeTime) * Time.deltaTime;
                ambienceClip.audioSource.volume = Mathf.Min(ambienceClip.audioSource.volume, ambienceClip.volume);
            }

            yield return 0;
        }

        // Set volume to the target volume for good measure
        foreach (AmbienceClip ambienceClip in ambience)
        {
            ambienceClip.audioSource.volume = ambienceClip.volume;
        }
    }

    private IEnumerator FadeOut()
    {
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            // Reduce the volume of each audio source
            foreach (AmbienceClip ambienceClip in ambience)
            {
                ambienceClip.audioSource.volume -= (ambienceClip.volume / fadeTime) * Time.deltaTime;
                ambienceClip.audioSource.volume = Mathf.Max(ambienceClip.audioSource.volume, 0.0f);
            }

            yield return 0;
        }

        // Set volume to zero for good measure
        foreach (AmbienceClip ambienceClip in ambience)
        {
            ambienceClip.audioSource.volume = 0.0f;
        }
    }
}

[System.Serializable]
public class AmbienceClip
{
    [Tooltip("The ambient sounds to play while within the trigger")]
    public AudioClip clip;

    [Tooltip("How loud the ambient sounds are")]
    [Range(0, 1)]
    public float volume = 1.0f;

    [HideInInspector]
    public AudioSource audioSource;
}
