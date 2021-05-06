using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TMP_Text text;
    public Color highlightTextColor;
    private Color defaultTextColor;

    private void Awake()
    {
        text = GetComponentInChildren<TMP_Text>();
        defaultTextColor = text.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.color = highlightTextColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.color = defaultTextColor;
    }
}
