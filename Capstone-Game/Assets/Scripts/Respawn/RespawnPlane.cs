using UnityEngine;

public class RespawnPlane : MonoBehaviour
{
    private Player.SquirrelController squirrel;

    private void Start()
    {
        squirrel = FindObjectOfType<Player.SquirrelController>();
    }

    private void Update()
    {
        if (squirrel.transform.position.y < transform.position.y)
        {
            squirrel.transform.position = Checkpoint.getRespawnPoint();
        }
    }
}
