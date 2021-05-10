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

    [Header("Public For Debug:")]
    //public float distanceToPlayerX;
    //public float distanceToPlayerY;
    //public float distanceToPlayerZ;
    //public float mouseX;
    //public float mouseY;
    public float finalInputX;
    public float finalInputY;
    //public float smoothX;
    //public float smoothY;

    private float rotY = 0.0f;
    private float rotX = 0.0f;
    private Vector3 FollowPos;

    
    void Awake()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Start()
    {
        UpdatePosition();
        StartCoroutine("StartCamReturnTimer");
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePosition();
    }

    void UpdatePosition()
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
    }

    private void LateUpdate()
    {
        CameraUpdater();
    }

    void CameraUpdater()
    {
        // Set camera target
        Transform target = cameraTarget.transform;

        // Move towards target
        float step = CameraMoveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);
    }

    /// <summary>
    /// Suspends execution for a given number of seconds
    /// </summary>
    /// <param name="seconds">The number of seconds to delay</param>
    /// <returns></returns>
    IEnumerator ReturnDelay(int seconds)
    {
        // Wait for the given time before returning
        yield return new WaitForSeconds(seconds);
    }

    private IEnumerator StartCamReturnTimer()
    {
        Debug.LogError("Starting " + Time.time);

        // Start delay before camera returns to center
        yield return StartCoroutine("ReturnCamToCenter");
        Debug.LogWarning("Done " + Time.time);
    }

}
