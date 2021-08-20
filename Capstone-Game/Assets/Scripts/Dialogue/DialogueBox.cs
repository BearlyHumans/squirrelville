using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class DialogueBox : MonoBehaviour
{
    [Tooltip("Reference to the dialogue box name text element")]
    public TMP_Text name;

    [Tooltip("Reference to the dialogue box content text element")]
    public TMP_Text text;

    [Tooltip("The delay in seconds between typing each letter")]
    public float typingSpeed;

    [Tooltip("Whether the dialogue box is currently visible")]
    [HideInInspector]
    public bool isDialogueOpen = false;

    private Dialogue dialogue;
    private int index = -1;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private bool doneSpeaking = true;

    private void Update()
    {
        if (PauseMenu.paused || dialogue == null) return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                isTyping = false;
                text.text = dialogue.sentences[index];
            }
            else
            {
                NextSentence();
            }
        }
    }

    public void SetDialogue(Dialogue dialogue)
    {
        this.dialogue = dialogue;
        name.text = dialogue.name;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        index = -1;
        isTyping = false;
        doneSpeaking = false;

        NextSentence();
    }

    private IEnumerator Type()
    {
        isTyping = true;
        isDialogueOpen = true;

        foreach (char letter in dialogue.sentences[index].ToCharArray())
        {
            text.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    public void NextSentence()
    {
        text.text = "";
        isDialogueOpen = false;

        if (index < dialogue.sentences.Length - 1)
        {
            index++;

            if (index == 0)
                dialogue.dialogueStart?.Invoke();
            else
            {
                dialogue.dialogueNext?.Invoke();
            }

            typingCoroutine = StartCoroutine(Type());
        }
        else if (!doneSpeaking)
        {
            doneSpeaking = true;
            dialogue.dialogueFinish?.Invoke();
        }
    }
}
