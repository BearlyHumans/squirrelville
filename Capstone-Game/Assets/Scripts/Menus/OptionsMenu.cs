using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions;
    private Resolution selectedResolution;
    public Animation anim;

    private void Start()
    {
        LoadSettings();
        InitResolutionDropdown();
    }

    private void LoadSettings()
    {
        musicVolumeSlider.value = PlayerPrefs.GetFloat("musicVolume", 1.0f);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat("sfxVolume", 1.0f);

        selectedResolution = new Resolution();
        selectedResolution.width = PlayerPrefs.GetInt("resolutionWidth", Screen.currentResolution.width);
        selectedResolution.height = PlayerPrefs.GetInt("resolutionHeight", Screen.currentResolution.height);

        Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreen);
    }

    private void InitResolutionDropdown()
    {
        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResIndex = 0;

        for (int i = resolutions.Length - 1; i >= 0; i--)
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
        audioMixer.SetFloat("musicVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("musicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("sfxVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("sfxVolume", volume);
    }

    public void SetResolution(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);

        PlayerPrefs.SetInt("resolutionWidth", res.width);
        PlayerPrefs.SetInt("resolutionHeight", res.height);
    }

    public void Back()
    {
        anim.Play("OptionsToMainMenu");
    }
}
