using UnityEngine;
using System.Collections;

public class BF_FPSDisplay : MonoBehaviour
{
    float deltaTime = 0.0f;

    private GUIStyle style = null;

    private bool ShowFps = false;

    private void Start()
    {
        ShowFps = true;
        style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        style.normal.textColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            ShowFps = !ShowFps;
        }

        if(ShowFps)
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        if (ShowFps)
        {
            int w = Screen.width, h = Screen.height;

            style.fontSize = h * 4 / 100;

            Rect rect = new Rect(0, 0, w, h * 2 / 100);

            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
            GUI.Label(rect, string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps), style);
        }
    }
}