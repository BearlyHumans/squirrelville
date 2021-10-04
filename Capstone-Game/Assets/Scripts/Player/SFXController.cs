using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXController : MonoBehaviour
{
    public int numberOfSources = 0;

    public List<AudioClip> sounds = new List<AudioClip>();

    private List<AudioSource> sources = new List<AudioSource>();
    private int lastSourcePlayed = 0;

    /// <summary> Play the sound from the start (good for landing). </summary>
    public void PlaySound(string soundName)
    {
        soundName = soundName.ToLower();
        foreach (AudioSource s in sources)
        {
            if (s.clip != null && s.clip.name.ToLower() == soundName)
            {
                s.Stop();
                s.Play();
                s.loop = false;
                return;
            }
        }

        foreach (AudioClip c in sounds)
        {
            if (c.name.ToLower() == soundName)
            {
                AudioSource source = GetBestSource();
                source.clip = c;
                source.Play();
                source.loop = false;
                return;
            }
        }
    }

    /// <summary> Play the sound if it isn't playing currently (good for triggering every frame). </summary>
    public void PlayOrContinueSound(string soundName)
    {
        soundName = soundName.ToLower();
        foreach (AudioSource s in sources)
        {
            if (s.clip != null && s.clip.name.ToLower() == soundName)
            {
                if (!s.isPlaying)
                    s.Play();
                s.loop = false;
                return;
            }
        }

        foreach (AudioClip c in sounds)
        {
            if (c.name.ToLower() == soundName)
            {
                AudioSource source = GetBestSource();
                source.clip = c;
                source.Play();
                source.loop = false;
                return;
            }
        }
    }

    /// <summary> Play the sound if there is a free source (same as PlayOrContinue otherwise). </summary>
    public void PlayIfQuiet(string soundName)
    {
        soundName = soundName.ToLower();
        int numPlaying = 0;
        foreach (AudioSource s in sources)
        {
            if (s.clip != null)
            {
                if (s.clip.name.ToLower() == soundName)
                {
                    if (!s.isPlaying)
                        s.Play();
                    s.loop = false;
                    return;
                }
                else if (s.isPlaying)
                    numPlaying += 1;
            }
        }

        if (numPlaying == numberOfSources)
            return;

        foreach (AudioClip c in sounds)
        {
            if (c.name.ToLower() == soundName)
            {
                AudioSource source = GetBestSource();
                source.clip = c;
                source.Play();
                source.loop = false;
                return;
            }
        }
    }

    /// <summary> Play the sound if there are no other sounds playing in this controller (same as PlayOrContinue otherwise). </summary>
    public void PlayIfSilent(string soundName)
    {
        soundName = soundName.ToLower();
        int numPlaying = 0;
        foreach (AudioSource s in sources)
        {
            if (s.isPlaying)
                numPlaying += 1;
        }

        if (numPlaying > 0)
            return;

        foreach (AudioSource s in sources)
        {
            if (s.clip != null && s.clip.name.ToLower() == soundName)
            {
                if (!s.isPlaying)
                    s.Play();
                s.loop = false;
                return;
            }
        }

        foreach (AudioClip c in sounds)
        {
            if (c.name.ToLower() == soundName)
            {
                AudioSource source = GetBestSource();
                source.clip = c;
                source.Play();
                source.loop = false;
                return;
            }
        }
    }

    /// <summary> Play the sound and set it to loop (good for events that only trigger at the start and end). </summary>
    public void LoopSound(string soundName)
    {
        soundName = soundName.ToLower();
        foreach (AudioSource s in sources)
        {
            if (s.clip != null && s.clip.name.ToLower() == soundName)
            {
                s.Play();
                s.loop = true;
                return;
            }
        }

        foreach (AudioClip c in sounds)
        {
            if (c.name.ToLower() == soundName)
            {
                AudioSource source = GetBestSource();
                source.clip = c;
                source.Play();
                source.loop = true;
                return;
            }
        }
    }

    /// <summary> Stop the sound immediately (good for cancelling other sounds when hit). </summary>
    public void StopSound(string soundName)
    {
        soundName = soundName.ToLower();
        foreach (AudioSource s in sources)
        {
            if (s.clip != null && s.clip.name.ToLower() == soundName)
            {
                s.Stop();
                s.loop = false;
                return;
            }
        }
    }

    /// <summary> Turn looping off for the sound so it stops after this playthrough (other half of loop). </summary>
    public void StopLoopingSound(string soundName)
    {
        soundName = soundName.ToLower();
        foreach (AudioSource s in sources)
        {
            if (s.clip != null && s.clip.name.ToLower() == soundName)
            {
                s.loop = false;
                return;
            }
        }
    }

    private AudioSource GetBestSource()
    {
        AudioSource source = null;
        for (int i = 0; i < sources.Count; ++i)
        {
            if (!sources[i].isPlaying)
            {
                source = sources[i];
                lastSourcePlayed = i;
                break;
            }
        }

        if (source == null)
        {
            lastSourcePlayed = (lastSourcePlayed + 1) % sources.Count;
            source = sources[lastSourcePlayed];
        }

        return source;
    }

    // Start is called before the first frame update
    void Awake()
    {
        AudioSource[] existingSources = GetComponents<AudioSource>();
        sources.AddRange(existingSources);

        if (sources.Count > numberOfSources)
        {
            numberOfSources = sources.Count;
            Debug.Log("Number of sources increased to number found in SFX controller");
        }
        else
        {
            for (int i = sources.Count; i < numberOfSources; ++i)
                sources.Add(gameObject.AddComponent<AudioSource>());
        }
    }

    public void Pause()
    {
        foreach (AudioSource s in sources)
        {
            if (!s.isPlaying)
                s.clip = null;

            if (s.clip != null)
                s.Pause();
        }
    }

    public void Resume()
    {
        foreach (AudioSource s in sources)
        {
            if (s.clip != null)
                s.Play();
        }
    }
}
