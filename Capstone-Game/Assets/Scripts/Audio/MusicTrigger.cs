using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class MusicTrigger : MonoBehaviour
{
    [Tooltip("The audio mixer group to play the music in")]
    public AudioMixerGroup group;

    [Tooltip("The music audio clip to play while within the trigger")]
    public AudioClip clip;

    [Tooltip("How loud the music is")]
    [Range(0, 1)]
    public float volume = 1.0f;

    [Tooltip("How many seconds for the music to fade in when entering the trigger")]
    [Min(0)]
    public float fadeInTime;

    [Tooltip("How many seconds for the music to fade out when exiting the trigger")]
    [Min(0)]
    public float fadeOutTime;

    [Header("Events")]
    public UnityEvent musicBegin;
    public UnityEvent musicEnd;

    private AudioSource audioSource;

    private static List<MusicTrigger> musicTriggers = new List<MusicTrigger>();
    private static MusicTrigger activeMusicTrigger;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = group;
        audioSource.clip = clip;
        audioSource.volume = 0.0f;
        audioSource.loop = true;
        audioSource.Play();

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (musicTriggers.IndexOf(this) > -1) return;

        musicTriggers.Add(this);

        if (activeMusicTrigger != null)
        {
            activeMusicTrigger.StopAllCoroutines();
        }

        if (activeMusicTrigger != null && activeMusicTrigger != this)
        {
            activeMusicTrigger.StartCoroutine(activeMusicTrigger.FadeTo(this));
        }
        else
        {
            StartCoroutine(FadeIn());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (musicTriggers.IndexOf(this) == -1) return;

        bool lastTrigger = musicTriggers[musicTriggers.Count - 1] == this;

        musicTriggers.Remove(this);

        if (lastTrigger && activeMusicTrigger != null)
        {
            activeMusicTrigger.StopAllCoroutines();

            if (musicTriggers.Count > 0)
            {
                activeMusicTrigger.StartCoroutine(FadeTo(musicTriggers[musicTriggers.Count - 1]));
            }
            else
            {
                activeMusicTrigger.StartCoroutine(activeMusicTrigger.FadeOut());
            }
        }
    }

    private IEnumerator FadeIn()
    {
        activeMusicTrigger = this;
        musicBegin.Invoke();

        while (audioSource.volume < volume)
        {
            audioSource.volume += (volume / fadeInTime) * Time.deltaTime;
            audioSource.volume = Mathf.Min(audioSource.volume, volume);
            yield return 0;
        }
    }

    private IEnumerator FadeOut()
    {
        while (audioSource.volume > 0)
        {
            audioSource.volume -= (volume / fadeOutTime) * Time.deltaTime;
            audioSource.volume = Mathf.Max(audioSource.volume, 0.0f);
            yield return 0;
        }

        activeMusicTrigger = null;
        musicEnd.Invoke();
    }

    public IEnumerator FadeTo(MusicTrigger musicTrigger)
    {
        yield return StartCoroutine(FadeOut());
        yield return musicTrigger.StartCoroutine(musicTrigger.FadeIn());
    }

    public void ManualFadeOut()
    {
        if (activeMusicTrigger != null)
        {
            activeMusicTrigger.StopAllCoroutines();
            activeMusicTrigger.StartCoroutine(activeMusicTrigger.FadeOut());
        }
    }

    public void ManualFadeIn()
    {
        if (activeMusicTrigger != null)
        {
            if (activeMusicTrigger != this)
            {
                activeMusicTrigger.StopAllCoroutines();
                activeMusicTrigger.StartCoroutine(activeMusicTrigger.FadeTo(this));
            }
        }
        else
        {
            StartCoroutine(FadeIn());
        }
    }
}
