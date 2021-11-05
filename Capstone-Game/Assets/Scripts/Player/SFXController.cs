using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXController : MonoBehaviour
{
    private int numberOfSources = 0;
    [Header("Change the max number of simultaneous sounds by duplicating one of the sources.")]
    public List<Sound> sounds = new List<Sound>();

    private List<AudioSource> sources = new List<AudioSource>();
    private List<Sound> currentlyPlaying = new List<Sound>();
    private int lastSourcePlayed = 0;
    private List<string> blockedSounds = new List<string>();

    /// <summary> Play the sound from the start (good for landing). </summary>
    public void PlaySound(string soundName)
    {
        if (blockedSounds.Contains(soundName))
            return;

        AudioSource AS = null;
        int i = -1;
        if (GetSourcePlayingSound(soundName, out AS, out i))
        {
            Sound s = currentlyPlaying[i];
            AS.clip = s.randomSound;
            AS.volume = s.volume;
            AS.pitch = s.randomPitch;
            AS.Stop();
            AS.Play();
            AS.loop = false;
            return;
        }

        AudioSource source = FindAndSetSound(soundName);
    }

    /// <summary> Play the sound if it isn't playing currently (good for triggering every frame). </summary>
    public void PlayOrContinueSound(string soundName)
    {
        if (blockedSounds.Contains(soundName))
            return;

        AudioSource AS = null;
        int i = -1;
        if (GetSourcePlayingSound(soundName, out AS, out i))
        {
            Sound s = currentlyPlaying[i];
            if (!AS.isPlaying)
            {
                AS.clip = s.randomSound;
                AS.volume = s.volume;
                AS.pitch = s.randomPitch;
                AS.Play();
                AS.loop = false;
            }
            return;
        }

        AudioSource source = FindAndSetSound(soundName);
    }

    /// <summary> Play the sound if it isn't playing currently (good for triggering every frame). </summary>
    public void PlayAswell(string soundName)
    {
        if (blockedSounds.Contains(soundName))
            return;

        AudioSource source = FindAndSetSound(soundName);
    }

    /// <summary> Play the sound if there is a free source (same as PlayOrContinue otherwise). </summary>
    public void PlayIfQuiet(string soundName)
    {
        if (blockedSounds.Contains(soundName))
            return;

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

        AudioSource source = FindAndSetSound(soundName);
    }

    /// <summary> Play the sound if there are no other sounds playing in this controller (same as PlayOrContinue otherwise). </summary>
    public void PlayIfSilent(string soundName)
    {
        if (blockedSounds.Contains(soundName))
            return;

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

        AudioSource source = FindAndSetSound(soundName);
    }

    /// <summary> Play the sound and set it to loop (good for events that only trigger at the start and end). </summary>
    public void LoopSound(string soundName)
    {
        if (blockedSounds.Contains(soundName))
            return;

        AudioSource AS = null;
        int i = -1;
        if (GetSourcePlayingSound(soundName, out AS, out i))
        {
            Sound s = currentlyPlaying[i];
            if (!AS.isPlaying)
            {
                AS.clip = s.randomSound;
                AS.volume = s.volume;
                AS.pitch = s.randomPitch;
                AS.Play();
                AS.loop = true;
            }
            return;
        }

        AudioSource source = FindAndSetSound(soundName);
        source.loop = true;
    }

    /// <summary> Stop the sound immediately (good for cancelling other sounds when hit). </summary>
    public void StopSound(string soundName)
    {
        AudioSource AS = null;
        int i = -1;
        if (GetSourcePlayingSound(soundName, out AS, out i))
        {
            AS.Stop();
            AS.loop = false;
        }
    }

    /// <summary> Pause the sound (can only be resumed by PlayOrContinue). </summary>
    public void PauseSound(string soundName)
    {
        AudioSource AS = null;
        int i = -1;
        if (GetSourcePlayingSound(soundName, out AS, out i))
        {
            AS.Pause();
        }
    }

    /// <summary> Turn looping off for the sound so it stops after this playthrough (other half of loop). </summary>
    public void StopLoopingSound(string soundName)
    {
        AudioSource AS = null;
        int i = -1;
        if (GetSourcePlayingSound(soundName, out AS, out i))
        {
            AS.loop = false;
        }
    }

    public void BlockSound(string soundName)
    {
        if (!blockedSounds.Contains(soundName))
            blockedSounds.Add(soundName);
    }

    public void UnBlockSound(string soundName)
    {
        blockedSounds.Remove(soundName);
    }

    private AudioSource GetBestSource(out int index)
    {
        AudioSource source = null;
        for (int i = 0; i < sources.Count; ++i)
        {
            if (!sources[i].isPlaying)
            {
                source = sources[i];
                lastSourcePlayed = i;
                index = i;
                return source;
            }
        }

        lastSourcePlayed = (lastSourcePlayed + 1) % sources.Count;
        source = sources[lastSourcePlayed];
        index = lastSourcePlayed;

        return source;
    }

    private AudioSource FindAndSetSound(string soundName)
    {
        soundName = soundName.ToLower();
        foreach (Sound s in sounds)
        {
            if (s.name.ToLower() == soundName)
            {
                int i = -1;
                AudioSource source = GetBestSource(out i);
                currentlyPlaying[i] = s;
                source.clip = s.randomSound;
                source.volume = s.volume;
                source.pitch = s.randomPitch;
                source.Play();
                source.loop = false;
                return source;
            }
        }
        return null;
    }

    private bool GetSourcePlayingSound(string soundName, out AudioSource source, out int index)
    {
        soundName = soundName.ToLower();
        source = null;
        index = -1;
        soundName = soundName.ToLower();
        int i = 0;
        foreach (AudioSource AS in sources)
        {
            if (currentlyPlaying[i] != null && currentlyPlaying[i].name != null && currentlyPlaying[i].name.ToLower() == soundName)
            {
                source = AS;
                index = i;
                return true;
            }
            ++i;
        }
        return false;
    }

    // Start is called before the first frame update
    void Awake()
    {
        AudioSource[] existingSources = GetComponents<AudioSource>();
        sources.AddRange(existingSources);
        numberOfSources = sources.Count;

        foreach (AudioSource AS in sources)
        {
            currentlyPlaying.Add(null);
        }

        foreach (Sound s in sounds)
        {
            if ((s.name == null || s.name == "") && s.numClips == 0)
                s.name = s.randomSound.name;
        }
    }

    /// <summary> Pause sounds when the game is paused. </summary>
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

    /// <summary> Resume sounds when the game is un-paused. </summary>
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
