using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputLogger : MonoBehaviour
{
    public bool print = false;

    void Start()
    {
        Debug.LogError("Input logger in this scene - delete if not needed.");
    }

    void Update()
    {
        if (print)
        {
            Debug.Log("~~~ Start of Frame ~~~");
            foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKey(vKey))
                {
                    Debug.Log("Pressed: " + vKey.ToString());
                }
            }
        }
    }
}
