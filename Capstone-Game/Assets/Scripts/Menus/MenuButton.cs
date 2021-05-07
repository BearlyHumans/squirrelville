using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TMP_Text text;
    public Color highlightTextColor;
    private Color defaultTextColor;
    public float highlightFontScale = 1.0f;
    private float defaultFontSize;

    private void Awake()
    {
        text = GetComponentInChildren<TMP_Text>();
        defaultTextColor = text.color;
        defaultFontSize = text.fontSize;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.color = highlightTextColor;
        text.fontSize = defaultFontSize * highlightFontScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.color = defaultTextColor;
        text.fontSize = defaultFontSize;
    }
}
