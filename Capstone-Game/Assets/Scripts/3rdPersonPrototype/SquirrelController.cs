using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SquirrelController : MonoBehaviour
{
    public SCReferences refs = new SCReferences();
    public SCSettings settings = new SCSettings();
    public SCTriggers triggers = new SCTriggers();

    public bool debugPause = false;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (CheckPause())
            return;
        Rotate();
        Move();
        Jump();
        //WallClimb();
    }

    private bool CheckPause()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (debugPause)
            {
                debugPause = false;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1;
            }
            else
            {
                debugPause = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0;
            }
        }

        return debugPause;
    }

    private void Rotate()
    {
        float xAxis = Input.GetAxis("Mouse X") * settings.mouseSense;
        float yAxis = Input.GetAxis("Mouse Y") * settings.mouseSense;

        transform.Rotate(0, xAxis, 0);
        //refs.head.Rotate(-yAxis, 0, 0);
        Vector3 eRot = refs.head.rotation.eulerAngles;
        eRot = new Vector3(ClampCam(eRot.x - yAxis), eRot.y, eRot.z);
        Quaternion rot = new Quaternion();
        rot.eulerAngles = eRot;
        refs.head.rotation = rot;

        refs.RB.angularVelocity = Vector3.zero;
    }

    public float ClampCam(float val)
    {
        if (val > settings.cameraClampMax)
            return settings.cameraClampMax;
        else if (val < settings.cameraClampMin)
            return settings.cameraClampMin;

        return val;
    }

    private void Move()
    {
        float xAxis = Input.GetAxis("Horizontal");
        float yAxis = Input.GetAxis("Vertical");
        float yVel = refs.RB.velocity.y;
        refs.RB.velocity = transform.forward * yAxis * settings.speed + transform.right * xAxis * settings.speed + Vector3.up * yVel;
    }

    private void Jump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (triggers.feet.triggered)
                refs.RB.velocity += transform.up * settings.jumpForce;
        }
    }

    private void WallClimb()
    {
        if (triggers.wallClimb.triggered)
        {
            if (Input.GetKey(KeyCode.W))
            {
                Vector3 vel = refs.RB.velocity;
                vel.y = settings.wallClimbForce;
                refs.RB.velocity = vel;
            }
        }
    }
}

[System.Serializable]
public class SCReferences
{
    public Rigidbody RB;
    public Transform head;
    public Camera camera;
}

[System.Serializable]
public class SCSettings
{
    public float speed = 1;
    public float mouseSense = 1;
    public float cameraClampMax = 50;
    public float cameraClampMin = 0;
    public float jumpForce = 1;
    public float wallClimbForce = 1;
}

[System.Serializable]
public class SCTriggers
{
    public MovementTrigger feet;
    public MovementTrigger wallClimb;
}
