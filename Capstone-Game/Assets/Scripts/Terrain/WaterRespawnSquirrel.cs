using UnityEngine;

public class WaterRespawnSquirrel : MonoBehaviour
{
    [Tooltip("The squirrel's SquirrelController")]
    public Player.SquirrelController squirrel;

    [Min(0)]
    [Tooltip("How far below the water the squirrel needs to fall before they are respawned")]
    public float depth;
    private Vector3 spawnPosition;

    private void Start()
    {
        spawnPosition = squirrel.transform.position;
    }

    private void Update()
    {
        if (squirrel.transform.position.y < transform.position.y - depth)
        {
            squirrel.transform.position = spawnPosition;
        }
    }
}
