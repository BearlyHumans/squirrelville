using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class OptionsMenu : MonoBehaviour
{
    [Header("Audio")]
    public AudioMixer audioMixer;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Mouse sensitivity")]
    public Slider mouseSensSlider;
    [Range(100, 200)]
    public float initialMouseSens = 250.0f;

    [Header("Miscellaneous")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public Animation anim;

    private Resolution[] resolutions;
    private Resolution selectedResolution;
    private LoadAudioSettings audioSettings;

    private void Start()
    {
        audioSettings = GameObject.FindObjectOfType<LoadAudioSettings>();

        LoadSettings();
        InitResolutionDropdown();
    }

    private void LoadSettings()
    {
        musicVolumeSlider.value = PlayerPrefs.GetFloat("musicVolume", audioSettings.initialMusicVolume);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat("sfxVolume", audioSettings.initialSfxVolume);
        fullscreenToggle.isOn = PlayerPrefs.GetInt("fullscreen", 1) == 1;

        selectedResolution = new Resolution();
        selectedResolution.width = PlayerPrefs.GetInt("resolutionWidth", Screen.currentResolution.width);
        selectedResolution.height = PlayerPrefs.GetInt("resolutionHeight", Screen.currentResolution.height);

        mouseSensSlider.value = PlayerPrefs.GetFloat("mouseSensitivity", initialMouseSens);
    }

    private void InitResolutionDropdown()
    {
        resolutions = Screen.resolutions.Select(resolution => new Resolution { width = resolution.width, height = resolution.height }).Distinct().Reverse().ToArray();

        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            Resolution res = resolutions[i];

            options.Add(res.width + "x" + res.height);

            if (res.width == selectedResolution.width && res.height == selectedResolution.height)
            {
                currentResIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("musicVolume", Mathf.Log10(volume * audioSettings.maxMusicVolume) * 20);
        PlayerPrefs.SetFloat("musicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("sfxVolume", Mathf.Log10(volume * audioSettings.maxSfxVolume) * 20);
        PlayerPrefs.SetFloat("sfxVolume", volume);
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat("mouseSensitivity", sensitivity);
    }

    public void SetResolution(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);

        PlayerPrefs.SetInt("resolutionWidth", res.width);
        PlayerPrefs.SetInt("resolutionHeight", res.height);
    }

    public void SetFullcreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
        PlayerPrefs.SetInt("fullscreen", fullscreen ? 1 : 0);
    }

    public void Back()
    {
        anim.Play("OptionsToMainMenu");
    }
}
