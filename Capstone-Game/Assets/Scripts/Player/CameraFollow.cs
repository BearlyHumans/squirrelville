using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float CameraMoveSpeed = 120.0f;
    public GameObject cameraTarget;
    public float clampAngle = 80.0f;
    public float inputSensitivity = 150.0f;
    public GameObject cameraObj;
    public GameObject playerObj;
    public bool invertY = false;

    private float rotY = 0.0f;
    private float rotX = 0.0f;

    [Header("Public For Debug:")]
    public float finalInputX;
    public float finalInputY;

    
    void Awake()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;
    }

    public void UpdateCamFromImput()
    {
        float inputX = Input.GetAxis("Mouse X");
        float inputY = Input.GetAxis("Mouse Y");
        if (invertY)
            inputY = -inputY;
        finalInputX = inputX;
        finalInputY = inputY;

        rotY += finalInputX * inputSensitivity * Time.deltaTime;
        rotX += finalInputY * inputSensitivity * Time.deltaTime;

        rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

        Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
        transform.rotation = localRotation;

        // Move towards target
        transform.position = Vector3.MoveTowards(transform.position, cameraTarget.transform.position, CameraMoveSpeed * Time.deltaTime);
    }
}
