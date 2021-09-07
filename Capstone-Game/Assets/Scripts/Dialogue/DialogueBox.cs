using System.Collections.Generic;
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
    private Dictionary<HashSet<char>, float> punctuations = new Dictionary<HashSet<char>, float>()
    {
        {new HashSet<char>() {'.', '!', '?'}, 0.6f},
        {new HashSet<char>() {',', ';', ':'}, 0.3f},
    };

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
        text.text = "";

        float t = 0;
        int charIndex = 0;
        string textToType = dialogue.entries[index].text;

        while (charIndex < textToType.Length)
        {
            int lastCharIndex = charIndex;

            t += Time.deltaTime * typingSpeed;

            charIndex = Mathf.FloorToInt(t);
            charIndex = Mathf.Clamp(charIndex, 0, textToType.Length);

            for (int i = lastCharIndex; i < charIndex; i++)
            {
                text.text = textToType.Substring(0, i + 1);

                bool isLast = i >= textToType.Length - 1;
                if (i < textToType.Length - 1 && IsPunctuation(textToType[i], out float waitTime) && !IsPunctuation(textToType[i + 1], out _))
                {
                    yield return new WaitForSeconds(waitTime);
                }
            }

            yield return null;
        }

        text.text = textToType;
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

    private bool IsPunctuation(char character, out float waitTime)
    {
        foreach (KeyValuePair<HashSet<char>, float> punctuationCategory in punctuations)
        {
            if (punctuationCategory.Key.Contains(character))
            {
                waitTime = punctuationCategory.Value;
                return true;
            }
        }

        waitTime = default;
        return false;
    }
}
