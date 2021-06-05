using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BF_RainActivator : MonoBehaviour
{
    public Material skyboxDefault;
    public Material skyboxRain;

    private Color fogDefault;
    public Color fogRain;

    private Color equatorColorDefault;
    public Color equatorColorRain;

    private void Awake()
    {
        fogDefault = RenderSettings.fogColor;
        equatorColorDefault = RenderSettings.ambientEquatorColor;
    }

    private void OnEnable()
    {
        EnableRain();
    }

    private void OnDisable()
    {
        DisableRain();
    }

    private void OnDestroy()
    {
        DisableRain();
    }

    private void EnableRain()
    {
        RenderSettings.fogColor = fogRain;
        RenderSettings.skybox = skyboxRain;
        RenderSettings.ambientEquatorColor = equatorColorRain;
    }

    private void DisableRain()
    {
        RenderSettings.fogColor = fogDefault;
        RenderSettings.skybox = skyboxDefault;
        RenderSettings.ambientEquatorColor = equatorColorDefault;
    }
}
