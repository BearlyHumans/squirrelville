using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(AudioSource))]
public class DialogueBox : MonoBehaviour
{
    [Tooltip("Reference to the dialogue box title text element")]
    public TMP_Text title;

    [Tooltip("Reference to the dialogue box content text element")]
    public TMP_Text text;

    [Tooltip("Reference to image element above the dialogue box")]
    public Image image;

    [Tooltip("The text element that is displayed when a dialogue entry is finished displaying")]
    public TMP_Text nextText;

    [Tooltip("The delay in seconds between typing each letter")]
    public float typingSpeed;

    [Tooltip("Can the player skip the typing animation")]
    public bool canSkipTyping;

    [Tooltip("Whether the dialogue box is currently visible")]
    [HideInInspector]
    public bool isDialogueOpen = false;

    private NPCDialogue dialogue;
    private int index = -1;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private CanvasGroup canvasGroup;
    private AudioSource audioSource;
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

        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (PauseMenu.paused) return;

        if (dialogue != null && wasDialogueOpen)
        {
            if (Input.anyKeyDown && !Input.GetButtonDown("Pause"))
            {
                if (isTyping)
                {
                    if (canSkipTyping)
                    {
                        StopCoroutine(typingCoroutine);
                        isTyping = false;
                        text.text = dialogue.GetDialogue().entries[index].text;
                        nextText.enabled = true;
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

    public void SetDialogue(NPCDialogue dialogue)
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
        Dialogue currentDialogue = dialogue.GetDialogue();
        DialogueEntry dialogueEntry = currentDialogue.entries[index];
        string textToType = dialogueEntry.text;

        while (charIndex < textToType.Length)
        {
            int lastCharIndex = charIndex;

            t += Time.deltaTime * typingSpeed;

            charIndex = Mathf.FloorToInt(t);
            charIndex = Mathf.Clamp(charIndex, 0, textToType.Length);

            for (int i = lastCharIndex; i < charIndex; i++)
            {
                text.text = textToType.Substring(0, i + 1);

                if (i < textToType.Length - 1 && IsPunctuation(textToType[i], out float waitTime) && !IsPunctuation(textToType[i + 1], out _))
                {
                    yield return new WaitForSeconds(waitTime);
                }
            }

            if (!audioSource.isPlaying)
            {
                AudioClipSet audioClipSet = dialogueEntry.audioClipSet ?? currentDialogue.audioClipSet;
                audioSource.pitch = audioClipSet.getRandomPitch();
                audioSource.PlayOneShot(audioClipSet.GetRandomAudioClip());
            }

            yield return null;
        }

        text.text = textToType;
        nextText.enabled = true;
        isTyping = false;
    }

    public void NextSentence()
    {
        text.text = "";
        image.enabled = false;
        nextText.enabled = false;
        isDialogueOpen = false;

        if (index < dialogue.GetDialogue().entries.Length - 1)
        {
            index++;

            if (index == 0)
                dialogue.dialogueStart?.Invoke();
            else
                dialogue.dialogueNext?.Invoke();

            isDialogueOpen = true;
            typingCoroutine = StartCoroutine(Type());

            Sprite sprite = dialogue.GetDialogue().entries[index].sprite;
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
