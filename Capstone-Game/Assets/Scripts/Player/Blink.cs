using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blink : MonoBehaviour
{


    public GameObject blinkObject;
    public float timeBetweenBlinks = 2f;
    private float initialStoredTime;


    void Start()
    {
        if (blinkObject == null)
            Debug.LogError("Please assign the blunk GameObject");

        if (blinkObject != null)
            blinkObject.SetActive(false);

        initialStoredTime = timeBetweenBlinks; //store starting value 
    }

    void Update()
    {
        timeBetweenBlinks -= Time.deltaTime;

        if (timeBetweenBlinks <= 0f)
        {
            if (blinkObject != null)
                blinkObject.SetActive(true);

            StartCoroutine("ResetBlink");
        }
    }

        IEnumerator ResetBlink()
        {
            yield return new WaitForSeconds(.15f);

            if (blinkObject != null)
                blinkObject.SetActive(false);

            timeBetweenBlinks = initialStoredTime - (Random.Range(-1f, 1f));
        }
}