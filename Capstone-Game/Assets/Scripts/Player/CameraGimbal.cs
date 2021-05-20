using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraGimbal : MonoBehaviour
{
    [Header("References:")]
    public GameObject cameraTarget;
    public Camera dollyCamera;
    public Transform belowPoint;

    private Transform CamObj
    {
        get { return dollyCamera.transform; }
    }

    [Header("Gimbal Settings:")]
    public float CameraMoveSpeed = 120.0f;
    public float upperWorldClampAngle = 70.0f;
    public float lowerWorldClampAngle = -89.0f;
    public float upperRelativeClampAngle = 150.0f;
    public float lowerRelativeClampAngle = 0.0f;
    public float relativeZoomAngle = 30.0f;
    public float inputSensitivity = 150.0f;
    public bool invertY = false;

    [Header("Dolly Settings:")]
    [SerializeField, Range(0.0f, 10.0f)]
    public float minDistance = 1.0f;
    [SerializeField, Range(0.0f, 10.0f)]
    public float maxDistance = 4.0f;
    public LayerMask sphereLayers;
    public float collisionPadding = 0.9f;
    public float smallSphereRadius = 1f;
    public float largeSphereRadius = 2f;
    
    [Header("Shared Settings:")]
    public LayerMask rayLayers;
    [SerializeField, Range(0.0f, 30.0f)]
    public float smooth = 10.0f;

    [Header("Public For Debug:")]
    public Vector3 dollyDirAdjusted;
    public float dollyDistance;
    public float finalInputX;
    public float finalInputY;

    public string DEBUGString = "Null";

    //Gimbal values
    private float rotY = 0.0f;
    private float rotX = 0.0f;
    private float difference = 0.0f;

    //Dolly values
    private Vector3 dollyDir;
    private Vector3 desiredCameraPos;


    void Awake()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;

        dollyDir = CamObj.localPosition.normalized;
        dollyDistance = CamObj.localPosition.magnitude;
    }

    /// <summary> Change the rotation of the camera parent based on inputs. </summary>
    public void UpdateCamRotFromImput()
    {
        //Get input and invert or transform it.
        float inputX = Input.GetAxis("Mouse X");
        float inputY = Input.GetAxis("Mouse Y");
        if (invertY)
            inputY = -inputY;
        finalInputX = inputX;
        finalInputY = inputY;


        rotY += finalInputX * inputSensitivity * Time.deltaTime;
        if (finalInputY > 0 || PlayerCanSeeBelowPoint())
            rotX += finalInputY * inputSensitivity * Time.deltaTime;


        difference = Vector3.Angle(Vector3.up, -cameraTarget.transform.forward);
        Debug.Log("difference: " + difference);
        /*
        relativeRotX = Vector3.SignedAngle(-cameraTarget.transform.forward, CamObj.up, CamObj.right);
        float oldAngle = relativeRotX;
        relativeRotX = Mathf.Clamp(relativeRotX, lowerRelativeClampAngle, upperRelativeClampAngle);
        float delta = relativeRotX - oldAngle;
        rotX += delta;
        */
        float oldRelativeAngle = rotX + difference;
        float newRelativeAngle = Mathf.Clamp(oldRelativeAngle, lowerRelativeClampAngle, upperRelativeClampAngle);
        Debug.Log(newRelativeAngle);
        float delta = newRelativeAngle - oldRelativeAngle;
        rotX += delta;
        rotX = Mathf.Clamp(rotX, lowerWorldClampAngle, upperWorldClampAngle);

        Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
        transform.rotation = localRotation;
    }

    /// <summary> Move the camera gameobject towards the target at the move speed. </summary>
    public void UpdateCamPos()
    {
        // Move towards target
        transform.position = Vector3.MoveTowards(transform.position, cameraTarget.transform.position, CameraMoveSpeed * Time.deltaTime);
    }

    /// <summary> Adjust the distance between the camera and the rotation parent based on the angle and nearby obstacles. </summary>
    public void UpdateDolly()
    {
        //Cause the camera to zoom in if it is below the zoom angle.
        float zoomedMax = maxDistance;
        if ((rotX + difference) < relativeZoomAngle)
        {
            float range = maxDistance - minDistance;
            float fraction = 1 - (((rotX + difference) - relativeZoomAngle) / (lowerRelativeClampAngle - relativeZoomAngle));
            zoomedMax = minDistance + range * fraction;
        }
            
        desiredCameraPos = transform.TransformPoint(dollyDir * zoomedMax);

        //Move the camera closer if certain bad conditions are found:
        RaycastHit hit;
        if (Physics.Linecast(transform.position, desiredCameraPos, out hit, rayLayers.value))
        { //If line of sight is blocked by the specified layer(s), move in quickly (10x smooth speed).
            dollyDistance = Mathf.Clamp((hit.distance - collisionPadding), minDistance, zoomedMax);
            if (hit.collider.CompareTag("CameraSolid"))
                CamObj.localPosition = Vector3.Lerp(CamObj.localPosition, dollyDir * dollyDistance, Time.deltaTime * smooth * 10);
        }
        else if (Physics.CheckSphere(CamObj.position, smallSphereRadius, sphereLayers.value))
        { //If the camera is VERY near or inside an object in specified layer(s), move in at a smooth speed.
            dollyDistance = Mathf.Clamp(dollyDistance - Time.deltaTime * smooth, minDistance, zoomedMax);
            CamObj.localPosition = Vector3.Lerp(CamObj.localPosition, dollyDir * dollyDistance, Time.deltaTime * smooth);
        }
        else if (!Physics.CheckSphere(CamObj.position, largeSphereRadius, sphereLayers.value))
        { //If the camera is NOT near any specified objects, move out again at the smooth speed.
            //(MUST be defined by a larger sphere than the previous check to prevent flickering)
            dollyDistance = zoomedMax;
            CamObj.localPosition = Vector3.Lerp(CamObj.localPosition, dollyDir * dollyDistance, Time.deltaTime * smooth);
        }
    }

    /// <summary> Checks if there is line of sight between the player (i.e. the camera parent's position) and a point a small distance below the actual camera. </summary>
    private bool PlayerCanSeeBelowPoint()
    {
        RaycastHit hit;
        if (Physics.Linecast(transform.position, belowPoint.position, out hit, rayLayers.value))
            return false;

        return true;
    }
}
