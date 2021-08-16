using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Dialogue : MonoBehaviour
{
    public TMP_Text text;
    public float typingSpeed;
    [TextArea] public string[] sentences;
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
