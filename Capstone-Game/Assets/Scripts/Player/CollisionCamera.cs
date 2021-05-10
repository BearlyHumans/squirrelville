using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionCamera : MonoBehaviour
{
    // Public Data
    [SerializeField, Range(0.0f,10.0f)]
    public float minDistance = 1.0f;
    [SerializeField, Range(0.0f, 10.0f)]
    public float maxDistance = 4.0f;
    [SerializeField, Range(0.0f, 30.0f)]
    public float smooth = 10.0f;
    public float collisionPadding = 0.9f;
    public Vector3 dollyDirAdjusted;
    public float distance;
    //public Player.PlayerController targetPlayer;

    // Encapsulated Data
    private Vector3 dollyDir;
    private Vector3 desiredCameraPos;

    // Start is called before the first frame update
    void Awake()
    {
        dollyDir = transform.localPosition.normalized;
        distance = transform.localPosition.magnitude;
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        desiredCameraPos = transform.parent.TransformPoint(dollyDir * maxDistance);
        RaycastHit hit;

        if(Physics.Linecast(transform.parent.position, desiredCameraPos, out hit))
        {
            //collisionPadding keeps camera out of the walls and floors
            distance = Mathf.Clamp((collisionPadding * hit.distance), minDistance, maxDistance); 
            transform.localPosition = dollyDir * distance;
        }
        else
        {
            distance = maxDistance;
            transform.localPosition = Vector3.Lerp(transform.localPosition, dollyDir * distance, Time.deltaTime * smooth);

        }
    }
}
