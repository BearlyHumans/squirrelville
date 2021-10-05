using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXController : MonoBehaviour
{
    public int numberOfSources = 0;

    public List<Sound> sounds = new List<Sound>();

    private List<AudioSource> sources = new List<AudioSource>();
    private int lastSourcePlayed = 0;

    /// <summary> Play the sound from the start (good for landing). </summary>
    public void PlaySound(string soundName)
    {
        soundName = soundName.ToLower();
        foreach (AudioSource AS in sources)
        {
            if (AS.clip != null && AS.clip.name.ToLower() == soundName)
            {
                foreach (Sound s in sounds)
                {
                    if (s.randomSound.name.ToLower() == soundName)
                    {
                        AS.clip = s.randomSound;
                        AS.volume = s.volume;
                        AS.pitch = s.randomPitch;
                    }
                }
                AS.Stop();
                AS.Play();
                AS.loop = false;
                return;
            }
        }

        foreach (Sound s in sounds)
        {
            if (s.randomSound.name.ToLower() == soundName)
            {
                AudioSource source = GetBestSource();
                source.clip = s.randomSound;
                source.volume = s.volume;
                source.pitch = s.randomPitch;
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
        foreach (AudioSource AS in sources)
        {
            if (AS.clip != null && AS.clip.name.ToLower() == soundName)
            {
                if (!AS.isPlaying)
                    AS.Play();
                AS.loop = false;
                return;
            }
        }

        foreach (Sound s in sounds)
        {
            if (s.randomSound.name.ToLower() == soundName)
            {
                AudioSource source = GetBestSource();
                source.clip = s.randomSound;
                source.volume = s.volume;
                source.pitch = s.randomPitch;
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
        foreach (AudioSource AS in sources)
        {
            if (AS.clip != null)
            {
                if (AS.clip.name.ToLower() == soundName)
                {
                    if (!AS.isPlaying)
                        AS.Play();
                    AS.loop = false;
                    return;
                }
                else if (AS.isPlaying)
                    numPlaying += 1;
            }
        }

        if (numPlaying == numberOfSources)
            return;

        foreach (Sound s in sounds)
        {
            if (s.randomSound.name.ToLower() == soundName)
            {
                AudioSource source = GetBestSource();
                source.clip = s.randomSound;
                source.volume = s.volume;
                source.pitch = s.randomPitch;
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

        foreach (Sound s in sounds)
        {
            if (s.randomSound.name.ToLower() == soundName)
            {
                AudioSource source = GetBestSource();
                source.clip = s.randomSound;
                source.volume = s.volume;
                source.pitch = s.randomPitch;
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
        foreach (AudioSource AS in sources)
        {
            if (AS.clip != null && AS.clip.name.ToLower() == soundName)
            {
                AS.Play();
                AS.loop = true;
                return;
            }
        }

        foreach (Sound s in sounds)
        {
            if (s.name.ToLower() == soundName)
            {
                AudioSource source = GetBestSource();
                source.clip = s.randomSound;
                source.volume = s.volume;
                source.pitch = s.randomPitch;
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
        foreach (AudioSource AS in sources)
        {
            if (AS.clip != null && AS.clip.name.ToLower() == soundName)
            {
                AS.Stop();
                AS.loop = false;
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

        foreach (Sound s in sounds)
        {
            if ((s.name == null || s.name == "") && s.numClips == 0)
                s.name = s.randomSound.name;
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

    [System.Serializable]
    public class Sound
    {
        public string name;
        [SerializeField]
        private List<AudioClip> sounds = new List<AudioClip>();
        [Range(0, 1)]
        public float volume = 1;
        [SerializeField]
        private Vector2 randomPitchRange = new Vector2(1, 1);
        public AudioClip randomSound
        {
            get { return sounds[Random.Range(0, sounds.Count)]; }
        }
        public float randomPitch
        {
            get { return Random.Range(randomPitchRange.x, randomPitchRange.y); }
        }
        public float numClips
        {
            get { return sounds.Count; }
        }
    }
}
