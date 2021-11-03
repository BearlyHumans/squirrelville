using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeToBlack : MonoBehaviour
{
    public static FadeToBlack singleton;

    public Image blackImage;

    void Awake()
    {
        if (singleton != null)
            Debug.LogError("Multiple FadeToBlack's in the scene");
        singleton = this;
    }

    public void BecomeOpaque(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(Opaque(duration));
    }

    public void BecomeOpaque()
    {
        blackImage.color = new Color(0, 0, 0, 1);
    }

    private IEnumerator Opaque(float duration)
    {
        if (duration == 0)
        {
            blackImage.color = new Color(0, 0, 0, 1);
            yield break;
        }

        float startTime = Time.time;
        float alpha = 0;
        while (alpha < 1)
        {
            alpha = (Time.time - startTime) / duration;
            blackImage.color = new Color(0, 0, 0, alpha);
            yield return new WaitForEndOfFrame();
        }

        blackImage.color = new Color(0, 0, 0, 1);
    }

    public void BecomeTransparent(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(Transparent(duration));
    }

    public void BecomeTransparent()
    {
        blackImage.color = new Color(0, 0, 0, 0);
    }

    private IEnumerator Transparent(float duration)
    {
        if (duration == 0)
        {
            blackImage.color = new Color(0, 0, 0, 0);
            yield break;
        }

        float startTime = Time.time;
        float alpha = 1;
        while (alpha > 0)
        {
            alpha = 1 - ((Time.time - startTime) / duration);
            blackImage.color = new Color(0, 0, 0, alpha);
            yield return new WaitForEndOfFrame();
        }

        blackImage.color = new Color(0, 0, 0, 0);
    }
}
