using UnityEngine;
using UnityEngine.Audio;

public class LoadAudioSettings : MonoBehaviour
{
    public AudioMixer audioMixer;
    private static bool loaded = false;

    [Header("APPLY CHANGES TO PREFAB!")]

    [Range(0, 1)]
    public float maxMusicVolume = 1.0f;

    [Range(0, 1)]
    public float maxSfxVolume = 1.0f;

    [Range(0, 1)]
    public float initialMusicVolume = 0.5f;

    [Range(0, 1)]
    public float initialSfxVolume = 0.5f;


    private void Start()
    {
        if (loaded) return;

        float musicVolume = PlayerPrefs.GetFloat("musicVolume", initialMusicVolume);
        float sfxVolume = PlayerPrefs.GetFloat("sfxVolume", initialSfxVolume);

        audioMixer.SetFloat("musicVolume", Mathf.Log10(musicVolume * maxMusicVolume) * 20);
        audioMixer.SetFloat("sfxVolume", Mathf.Log10(sfxVolume * maxSfxVolume) * 20);

        loaded = true;
    }
}
