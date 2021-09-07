using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class DialogueBox : MonoBehaviour
{
    [Tooltip("Reference to the dialogue box title text element")]
    public TMP_Text title;

    [Tooltip("Reference to the dialogue box content text element")]
    public TMP_Text text;

    [Tooltip("Reference to image element above the dialogue box")]
    public Image image;

    [Tooltip("The delay in seconds between typing each letter")]
    public float typingSpeed;

    [Tooltip("Can the player skip the typing animation")]
    public bool canSkipTyping;

    [Tooltip("Whether the dialogue box is currently visible")]
    [HideInInspector]
    public bool isDialogueOpen = false;

    private Dialogue dialogue;
    private int index = -1;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private CanvasGroup canvasGroup;
    private bool wasDialogueOpen = false;

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
    }

    private void Update()
    {
        if (PauseMenu.paused) return;

        if (dialogue != null && wasDialogueOpen)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (isTyping)
                {
                    if (canSkipTyping)
                    {
                        StopCoroutine(typingCoroutine);
                        isTyping = false;
                        text.text = dialogue.entries[index].text;
                    }
                }
                else
                {
                    NextSentence();
                }
            }
        }

        wasDialogueOpen = isDialogueOpen;
    }

    public void SetDialogue(Dialogue dialogue)
    {
        this.dialogue = dialogue;
        title.text = dialogue.name;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        index = -1;
        isTyping = false;
        canvasGroup.alpha = 1;

        NextSentence();
    }

    private IEnumerator Type()
    {
        isTyping = true;

        foreach (char letter in dialogue.entries[index].text.ToCharArray())
        {
            text.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    public void NextSentence()
    {
        text.text = "";
        image.enabled = false;
        isDialogueOpen = false;

        if (index < dialogue.entries.Length - 1)
        {
            index++;

            if (index == 0)
                dialogue.dialogueStart?.Invoke();
            else
                dialogue.dialogueNext?.Invoke();

            isDialogueOpen = true;
            typingCoroutine = StartCoroutine(Type());

            Sprite sprite = dialogue.entries[index].sprite;
            if (sprite != null)
            {
                image.enabled = true;
                image.sprite = sprite;
            }
        }
        else
        {
            canvasGroup.alpha = 0;
            dialogue.dialogueFinish?.Invoke();
            dialogue = null;
        }
    }
}
