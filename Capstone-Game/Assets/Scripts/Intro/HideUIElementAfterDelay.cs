using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class HideUIElementAfterDelay : MonoBehaviour
{
    [Tooltip("How long should the UI element be visible for in seconds")]
    [Min(0)]
    public float delay;

    [Tooltip("How long should the UI element fade out for in seconds (0 to disable)")]
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
