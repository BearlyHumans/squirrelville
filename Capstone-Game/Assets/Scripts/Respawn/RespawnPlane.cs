using UnityEngine;

public class RespawnPlane : MonoBehaviour
{
    private Player.SquirrelController squirrel;
    private Food[] foods;

    private void Start()
    {
        squirrel = FindObjectOfType<Player.SquirrelController>();
        foods = FindObjectsOfType<Food>();
    }

    private void Update()
    {
        if (squirrel.transform.position.y < transform.position.y)
        {
            squirrel.transform.position = Checkpoint.getRespawnPoint();
        }

        foreach (Food food in foods)
        {
            if (food.transform.position.y < transform.position.y)
            {
                food.respawnSelf();
            }
        }
    }
}
