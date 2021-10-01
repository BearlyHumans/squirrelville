using UnityEngine;

public class RespawnPlane : MonoBehaviour
{
    private Player.SquirrelController squirrel;
    private Vector3 spawnPosition;

    private void Start()
    {
        squirrel = FindObjectOfType<Player.SquirrelController>();
        spawnPosition = squirrel.transform.position;
    }

    private void Update()
    {
        if (squirrel.transform.position.y < transform.position.y)
        {
            squirrel.transform.position = spawnPosition;
        }
    }
}
