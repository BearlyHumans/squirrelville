using UnityEngine;
using UnityEngine.Events;

public class Checkpoint : MonoBehaviour
{
    [Tooltip("The point to respawn the squirrel at.")]
    public Transform respawnPoint;

    [Tooltip("The priority level of this checkpoint. The checkpoint is set only if its priority is higher or equal to the current checkpoint's priority.")]
    public int priority = 0;

    [Header("Events")]
    public UnityEvent checkpointSet;
    public UnityEvent checkpointUnset;


    private static Checkpoint currentCheckpoint;
    private static Vector3 defaultRespawnPoint;

    private void Start()
    {
        if (defaultRespawnPoint == null)
        {
            defaultRespawnPoint = FindObjectOfType<Player.SquirrelController>().transform.position;
        }

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currentCheckpoint == null || (currentCheckpoint != this && priority >= currentCheckpoint.priority))
        {
            if (currentCheckpoint != null)
            {
                currentCheckpoint.checkpointUnset.Invoke();
            }

            currentCheckpoint = this;
            checkpointSet.Invoke();
        }
    }

    public static Vector3 getRespawnPoint()
    {
        return currentCheckpoint?.respawnPoint.position ?? defaultRespawnPoint;
    }
}
