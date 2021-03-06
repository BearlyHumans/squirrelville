using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraGimbal : MonoBehaviour
{
    [Header("References:")]
    [Header("Usefull Settings:")]
    public Transform cameraTarget;
    public Camera dollyCamera;
    public Transform belowPoint;
    public List<Renderer> targetRenderers = new List<Renderer>();
    private bool targetInvisible = false;

    [Header("Control Settings:")]
    [Tooltip("Uses 'zoom when at low angle' system when true, or 'button-toggle zoom' when false.")]
    public bool angleRelativeZoom = false;
    [Tooltip("The new maximum distance when the zoom toggle is pressed - larger to zoom out (e.g. 2), smaller to zoom in (e.g. 0.3).")]
    public float buttonZoomMultiplier = 0.3f;
    public bool invertY = false;

    [Header("FOV Settings:")]
    [Tooltip("FOV for Dashing")]
    public float alternateFOV = 65;
    private float normalFOV = 60;

    [Header("Obstacle Avoidance Settings:")]
    [Tooltip("Should contain layers of objects which have small and fiddly hitboxes, or which the player often goes around or under (e.g. chairs, trees, people).")]
    public LayerMask zoomToAvoidLayers;
    [Tooltip("Should contain layers of objects which are large, unmoving and well defined (e.g. the ground, rocks, houses).")]
    public LayerMask noClippingEverLayers;
    [Tooltip("Size of the virtual collision sphere around the camera - i.e. how close the camera can get to obstacles (shown with solid blue gizmo).")]
    public float collisionSphereRadius = 0.1f;
    [Tooltip("Size of the virtual collision sphere around the camera - i.e. how close the camera can get to obstacles (shown with solid blue gizmo).")]
    public float targetInvisibleRadius = 0.15f;

    [Header("Other Settings:")]
    [Tooltip("Roughly the time it takes the camera to catch up to the players position/velocity. Higher means more smoothing.")]
    [SerializeField, Range(0.0f, 1.0f)]
    public float translationSmoothing = 0.1f;
    public bool cameraSmoothThreshold = false;
    [Tooltip("How fast the camera rotates when you move your mouse/joystick")]
    public float cameraSensitivity = 150.0f;

    private Transform CamObj
    {
        get { return dollyCamera.transform; }
    }

    public CameraProSettings programmerSettings = new CameraProSettings();

    [System.Serializable]
    public class CameraProSettings
    {
        [Header("~~~ PROGRAMMER SETTINGS ~~~")]
        [SerializeField, Range(0.0f, 30.0f)]
        public float smooth = 10.0f;

        [Header("Gimbal Settings:")]
        public float upperWorldClampAngle = 70.0f;
        public float lowerWorldClampAngle = -89.0f;
        public float upperRelativeClampAngle = 150.0f;
        public float lowerRelativeClampAngle = 0.0f;
        public float relativeZoomAngle = 30.0f;

        [Header("Dolly Settings:")]
        [SerializeField, Range(0.0f, 10.0f)]
        public float minDistance = 0.4f;
        [SerializeField, Range(0.0f, 10.0f)]
        public float maxDistance = 2f;
        public float collisionPadding = 0.9f;
        public float largeSphereMultiplier = 2f;
    }

    private Vector3 dollyDirAdjusted;
    private float dollyDistance;
    private float finalInputX;
    private float finalInputY;

    public bool UseRelativeAngles
    {
        get { return useRelativeAngles; }
        set { useRelativeAngles = value; }
    }

    //Gimbal values
    private bool useRelativeAngles = true;
    private float rotY = 0.0f;
    private float rotX = 0.0f;
    private float playerAngle = 0.0f;

    //Dolly values
    private Vector3 dollyDir;
    private Vector3 desiredCameraPos;

    private Vector3 velocity = Vector3.zero;

    void Awake()
    {
        Vector3 rot = transform.rotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;

        normalFOV = dollyCamera.fieldOfView;

        dollyDir = CamObj.localPosition.normalized;
        dollyDistance = CamObj.localPosition.magnitude;
    }

    public void SwapToAlternateFOV()
    {
        dollyCamera.fieldOfView = alternateFOV;
    }

    public void ResetToNormalFOV()
    {
        dollyCamera.fieldOfView = normalFOV;
    }

    public void SetToCustomFOV(float newFOV)
    {
        dollyCamera.fieldOfView = newFOV;
    }

    /// <summary> Change the rotation of the camera parent based on inputs. </summary>
    public void UpdateCamRotFromInput()
    {
        //Get input and invert or transform it.
        float inputX = Input.GetAxis("Mouse X");
        float inputY = Input.GetAxis("Mouse Y");
        if (invertY)
            inputY = -inputY;
        finalInputX = inputX;
        finalInputY = inputY;

        Vector3 oldPos = dollyCamera.transform.position;

        //Y (Left Right)
        float deltaY = finalInputX * cameraSensitivity * Time.deltaTime;
        rotY += deltaY;

        //X (Up Down)
        //if (finalInputY > 0 || PlayerCanSeeBelowPoint())
        rotX += finalInputY * cameraSensitivity * Time.deltaTime;

        if (useRelativeAngles)
            playerAngle = Vector3.Angle(Vector3.up, cameraTarget.up);
        else
            playerAngle = 0;
        float oldRelativeAngle = rotX + playerAngle;
        float newRelativeAngle = Mathf.Clamp(oldRelativeAngle, programmerSettings.lowerRelativeClampAngle, programmerSettings.upperRelativeClampAngle);
        float deltaX = newRelativeAngle - oldRelativeAngle;
        deltaX = Mathf.Clamp(rotX + deltaX, programmerSettings.lowerWorldClampAngle, programmerSettings.upperWorldClampAngle) - rotX;
        rotX += deltaX;

        Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
        transform.rotation = localRotation;
    }

    public void SetSensitivity(float sensitivity)
    {
        cameraSensitivity = sensitivity;
    }

    /// <summary> Move the camera gameobject towards the target at the move speed. </summary>
    public void UpdateCamPos()
    {
        // Move towards target
        if (!cameraSmoothThreshold || (cameraTarget.position - transform.position).sqrMagnitude > 0.0005f)
            transform.position = Vector3.SmoothDamp(transform.position, cameraTarget.position, ref velocity, translationSmoothing);
    }

    public float UpdateDollyToggleZoom()
    {
        if (Input.GetButton("Zoom"))
        {
            return programmerSettings.maxDistance * buttonZoomMultiplier;
        }

        return programmerSettings.maxDistance;
    }

    public float UpdateDollyAngleZoom()
    {
        //Cause the camera to zoom in if it is below the zoom angle.
        float zoomedMax = programmerSettings.maxDistance;
        if ((rotX + playerAngle) < programmerSettings.relativeZoomAngle)
        {
            float range = programmerSettings.maxDistance - programmerSettings.minDistance;
            float fraction = 1 - (((rotX + playerAngle) - programmerSettings.relativeZoomAngle) / (programmerSettings.lowerRelativeClampAngle - programmerSettings.relativeZoomAngle));
            zoomedMax = programmerSettings.minDistance + range * fraction;
        }

        return zoomedMax;
    }

    /// <summary> Adjust the distance between the camera and the rotation parent based on the angle and nearby obstacles. </summary>
    public void UpdateDolly()
    {
        float zoomedMax;
        if (angleRelativeZoom)
        {
            zoomedMax = UpdateDollyAngleZoom();
        }
        else
        {
            zoomedMax = UpdateDollyToggleZoom();
        }

        desiredCameraPos = transform.TransformPoint(dollyDir * zoomedMax);

        float dist1 = zoomedMax;
        float dist2 = zoomedMax;
        float dist3 = zoomedMax;

        Vector3 dir = CamObj.position - transform.position;
        Vector3 offset = dir.normalized * collisionSphereRadius;
        RaycastHit hit;
        if (Physics.Linecast(transform.position, CamObj.position + offset * 2f, out hit, noClippingEverLayers))
        {
            float dist = Vector3.Distance(transform.position, hit.point - offset);
            CamObj.localPosition = dollyDir * dist;
        }
        else
        {
            if (Physics.CheckSphere(CamObj.position, collisionSphereRadius, noClippingEverLayers))
            { //If the camera is VERY near or inside an object in specified layer(s), move in at a smooth speed.
                dollyDistance = Mathf.Clamp(dollyDistance - collisionSphereRadius * 0.5f, programmerSettings.minDistance, zoomedMax);
                CamObj.localPosition = Vector3.Lerp(CamObj.localPosition, dollyDir * dollyDistance, Time.deltaTime * programmerSettings.smooth * 10);
            }

            //Move the camera closer if certain bad conditions are found:
            if (Physics.Linecast(transform.position, desiredCameraPos, out hit, zoomToAvoidLayers))
            { //If line of sight is blocked by the specified layer(s), move in quickly (10x smooth speed).
                dollyDistance = Mathf.Clamp((hit.distance - programmerSettings.collisionPadding), programmerSettings.minDistance, zoomedMax);
                CamObj.localPosition = Vector3.Lerp(CamObj.localPosition, dollyDir * dollyDistance, Time.deltaTime * programmerSettings.smooth * 10);
            }
            else if (Physics.CheckSphere(CamObj.position, collisionSphereRadius, zoomToAvoidLayers))
            { //If the camera is VERY near or inside an object in specified layer(s), move in at a smooth speed.
                dollyDistance = Mathf.Clamp(dollyDistance - Time.deltaTime * programmerSettings.smooth, programmerSettings.minDistance, zoomedMax);
                CamObj.localPosition = Vector3.Lerp(CamObj.localPosition, dollyDir * dollyDistance, Time.deltaTime * programmerSettings.smooth);
            }
            else if (!Physics.CheckSphere(CamObj.position, collisionSphereRadius * programmerSettings.largeSphereMultiplier, zoomToAvoidLayers))
            { //If the camera is NOT near any specified objects, move out again at the smooth speed.
              //(MUST be defined by a larger sphere than the previous check to prevent flickering)
                dollyDistance = zoomedMax;
                CamObj.localPosition = Vector3.Lerp(CamObj.localPosition, dollyDir * dollyDistance, Time.deltaTime * programmerSettings.smooth);
            }
        }

        if (Vector3.Distance(CamObj.position, transform.position) < targetInvisibleRadius)
        {
            foreach (Renderer MR in targetRenderers)
                MR.enabled = false;
            targetInvisible = true;
        }
        else
        {
            if (targetInvisible)
            {
                foreach (Renderer MR in targetRenderers)
                    MR.enabled = true;
                targetInvisible = false;
            }
        }
    }

    private void RotateToUnblockedPoint(float degPerMove)
    {
        //bool foundOpenSpace = false;
        float interval = Mathf.Abs(degPerMove / 2f);
        interval = Mathf.Max(interval, 0.5f);
        int i = 0;
        for (float offset = 0; offset < 90 && i < 20; offset += interval)
        {
            //Positive angle;
            Quaternion localRotation = Quaternion.Euler(rotX, rotY + offset, 0.0f);
            transform.rotation = localRotation;
            if (!Physics.CheckSphere(CamObj.position, collisionSphereRadius, noClippingEverLayers.value))
            {
                //foundOpenSpace = true;
                rotY = rotY + offset;
                break;
            }

            //Positive angle;
            localRotation = Quaternion.Euler(rotX, rotY - offset, 0.0f);
            transform.rotation = localRotation;
            if (!Physics.CheckSphere(CamObj.position, collisionSphereRadius, noClippingEverLayers.value))
            {
                //foundOpenSpace = true;
                rotY = rotY - offset;
                break;
            }

            i += 1;
        }
    }

    /// <summary> Checks if there is line of sight between the player (i.e. the camera parent's position) and a point a small distance below the actual camera. </summary>
    private bool PlayerCanSeeBelowPoint()
    {
        RaycastHit hit;
        if (Physics.Linecast(transform.position, belowPoint.position, out hit, noClippingEverLayers.value))
            return false;

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(dollyCamera.transform.position, collisionSphereRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(dollyCamera.transform.position, collisionSphereRadius * programmerSettings.largeSphereMultiplier);
        Gizmos.DrawLine(transform.position, CamObj.transform.position);
        Gizmos.DrawLine(transform.position, belowPoint.position);
    }
}
