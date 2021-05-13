using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDolly : MonoBehaviour
{
    // Public Data
    public Camera dollyCamera;
    [SerializeField, Range(0.0f,10.0f)]
    public float minDistance = 1.0f;
    [SerializeField, Range(0.0f, 10.0f)]
    public float maxDistance = 4.0f;
    [SerializeField, Range(0.0f, 30.0f)]
    public float smooth = 10.0f;
    public LayerMask rayLayers;
    public LayerMask sphereLayers;
    public float collisionPadding = 0.9f;
    public float smallSphereRadius = 1f;
    public float largeSphereRadius = 2f;
    public Vector3 dollyDirAdjusted;
    public float distance;
    //public Player.PlayerController targetPlayer;

    public string DEBUGString = "Null";

    // Encapsulated Data
    private Vector3 dollyDir;
    private Vector3 desiredCameraPos;

    private Transform DCam
    {
        get { return dollyCamera.transform; }
    }

    // Start is called before the first frame update
    void Awake()
    {
        dollyDir = DCam.localPosition.normalized;
        distance = DCam.localPosition.magnitude;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDolly();
    }

    private void UpdateDolly()
    {
        desiredCameraPos = transform.TransformPoint(dollyDir * maxDistance);
        RaycastHit hit;

        if (Physics.Linecast(transform.position, desiredCameraPos, out hit, rayLayers.value))
        {
            DEBUGString = "Cast hit non-solid";
            distance = Mathf.Clamp((hit.distance - collisionPadding), minDistance, maxDistance);
            if (hit.collider.CompareTag("CameraSolid"))
            {
                DEBUGString = "Cast hit solid";
                DCam.localPosition = Vector3.Lerp(DCam.localPosition, dollyDir * distance, Time.deltaTime * smooth * 10);
            }
        }
        else if (!Physics.CheckSphere(DCam.position, largeSphereRadius, sphereLayers.value))
        {
            DEBUGString = "Large Sphere";
            //Allow camera to move back.
            distance = maxDistance;
            DCam.localPosition = Vector3.Lerp(DCam.localPosition, dollyDir * distance, Time.deltaTime * smooth);
        }
        else if (Physics.CheckSphere(DCam.position, smallSphereRadius, sphereLayers.value))
        {
            DEBUGString = "Small Sphere";
            //Move camera forwards.
            distance = Mathf.Clamp(distance - Time.deltaTime * smooth, minDistance, maxDistance);
            DCam.localPosition = Vector3.Lerp(DCam.localPosition, dollyDir * distance, Time.deltaTime * smooth);
        }
    }
}
