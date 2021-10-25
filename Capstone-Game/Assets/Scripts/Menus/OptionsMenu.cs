using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class OptionsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider cameraSensSlider;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public Animation anim;

    private Resolution[] resolutions;
    private Resolution selectedResolution;
    private LoadAudioSettings audioSettings;

    private void Update()
    {
        if (MenuManager.ButtonsEnabled() && Input.GetKey(KeyCode.Escape))
        {
            Back();
        }
    }

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

        cameraSensSlider.value = PlayerPrefs.GetFloat("cameraSensitivity", 250);
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

    public void SetCameraSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat("cameraSensitivity", sensitivity);
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
