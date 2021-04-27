using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static Canvas singleton;

    void Awake()
    {
        if (singleton == null)
            singleton = GetComponent<Canvas>();
        else
            Destroy(gameObject);
    }
}
