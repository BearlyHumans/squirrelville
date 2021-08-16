using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Dialogue : MonoBehaviour
{
    [Tooltip("Reference to the dialogue box text element")]
    public TMP_Text text;

    [Tooltip("The delay in seconds between typing each letter")]
    public float typingSpeed;

    [Tooltip("A list of sentences to display as part of the dialogue")]
    [TextArea]
    public string[] sentences;

    [Tooltip("Event evoked when the dialogue is done")]
    public UnityEvent dialogueDone;

    private int index = -1;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private bool doneSpeaking = false;

    private void Start()
    {
        NextSentence();
    }

    private void Update()
    {
        if (PauseMenu.paused) return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                isTyping = false;
                text.text = sentences[index];
            }
            else
            {
                NextSentence();
            }
        }
    }

    private IEnumerator Type()
    {
        isTyping = true;

        foreach (char letter in sentences[index].ToCharArray())
        {
            text.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    public void NextSentence()
    {
        text.text = "";

        if (index < sentences.Length - 1)
        {
            index++;
            typingCoroutine = StartCoroutine(Type());
        }
        else if (!doneSpeaking)
        {
            doneSpeaking = true;
            dialogueDone?.Invoke();
        }
    }
}
