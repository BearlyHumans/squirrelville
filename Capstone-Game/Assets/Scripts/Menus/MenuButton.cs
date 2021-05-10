using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TMP_Text text;
    private Button button;
    public Color highlightTextColor;
    private Color defaultTextColor;
    public float highlightFontScale = 1.0f;
    private float defaultFontSize;
    public bool underlineOnHighlight = false;
    private FontStyles defaultFontStyle;

    private void Awake()
    {
        text = GetComponentInChildren<TMP_Text>();
        button = GetComponent<Button>();
        defaultTextColor = text.color;
        defaultFontSize = text.fontSize;
        defaultFontStyle = text.fontStyle;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.color = highlightTextColor;
        text.fontSize = defaultFontSize * highlightFontScale;

        if (underlineOnHighlight)
            text.fontStyle |= FontStyles.Underline;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.color = defaultTextColor;
        text.fontSize = defaultFontSize;
        text.fontStyle = defaultFontStyle;
    }
}
