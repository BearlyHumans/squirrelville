using System.Collections;
using UnityEngine;
using TMPro;

public class Dialogue : MonoBehaviour
{
    public TMP_Text text;
    [TextArea] public string[] sentences;
    private int index = -1;
    public float typingSpeed;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private void Start()
    {
        NextSentence();
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

    public void NextSentence()
    {
        text.text = "";

        if (index < sentences.Length - 1)
        {
            index++;
            typingCoroutine = StartCoroutine(Type());
        }
    }
}
