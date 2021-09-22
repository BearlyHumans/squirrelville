using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    private Vector3 spawnPoint;
    private static List<Food> foodList = new List<Food>();

    private bool isActive;

    private void Awake()
    {
        foodList.Add(this);

        spawnPoint = transform.position;
    }

    public void Update() 
    {
        if(gameObject.activeSelf)
        {
            print("is active");
        }
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

}
