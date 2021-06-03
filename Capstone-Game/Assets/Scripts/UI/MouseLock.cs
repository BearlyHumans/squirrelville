using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLock : MonoBehaviour
{

    void Update()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;


    }
}
