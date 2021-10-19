using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class HideAfterDelay : MonoBehaviour
{
    [Min(0)]
    public float delay;

    [Min(0)]
    public float fadeOutTime;

    private CanvasGroup canvasGroup;

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        if (Time.timeSinceLevelLoad >= delay)
        {
            canvasGroup.alpha -= fadeOutTime > 0 ? Time.deltaTime / fadeOutTime : 1;

            if (canvasGroup.alpha <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
