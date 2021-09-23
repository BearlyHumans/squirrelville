using UnityEngine;
using UnityEngine.Audio;

public class LoadAudioSettings : MonoBehaviour
{
    public AudioMixer audioMixer;
    private static bool loaded = false;

    private void Start()
    {
        if (!loaded)
        {
            float musicVolume = PlayerPrefs.GetFloat("musicVolume", 1.0f);
            float sfxVolume = PlayerPrefs.GetFloat("sfxVolume", 1.0f);

            audioMixer.SetFloat("musicVolume", Mathf.Log10(musicVolume) * 20);
            audioMixer.SetFloat("sfxVolume", Mathf.Log10(sfxVolume) * 20);

            loaded = true;
        }

        Destroy(gameObject);
    }
}
