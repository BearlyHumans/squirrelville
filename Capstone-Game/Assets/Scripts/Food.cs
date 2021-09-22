using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    private Vector3 spawnPoint;
    private static List<Food> foodList = new List<Food>();

    public bool isActive;


    private void Awake()
    {
        foodList.Add(this);

        spawnPoint = transform.position;
        print(transform.position);
        print(spawnPoint);
    }


    public bool isEaten()
    {
        return !gameObject.activeInHierarchy;
    }

    public static Food[] GetFood()
    {
        return foodList.ToArray();
    }

    public static bool IsFood(GameObject obj)
    {
        int foodLayer = LayerMask.NameToLayer("Food");
        return obj.layer == foodLayer;
    }
    public void respawn()
    {
        Invoke("respawnSelf", 10);
    }

    public void respawnSelf()
    {
        gameObject.SetActive(true);
        gameObject.transform.position = spawnPoint;

    }

}
