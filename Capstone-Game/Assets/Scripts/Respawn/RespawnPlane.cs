using UnityEngine;

public class RespawnPlane : MonoBehaviour
{
    private Player.SquirrelController squirrel;
    private Food[] foods;
    private GameObject[] npcFoods;

    private void Start()
    {
        squirrel = FindObjectOfType<Player.SquirrelController>();
        foods = FindObjectsOfType<Food>();
    }

    private void Update()
    {
        npcFoods = GameObject.FindGameObjectsWithTag("NPCFood");

        if (squirrel.transform.position.y < transform.position.y)
        {
            Respawn();
        }

        foreach (Food food in foods)
        {
            if (food.transform.position.y < transform.position.y)
            {
                food.respawnSelf();
            }
        }

        foreach (GameObject npcfood in npcFoods)
        {
            if (npcfood.transform.position.y < transform.position.y)
            {
                Destroy(npcfood);
            }
        }
    }

    private void Respawn()
    {
        squirrel.transform.position = Checkpoint.getRespawnPoint();
        squirrel.refs.RB.velocity = Vector3.zero;
    }
}
