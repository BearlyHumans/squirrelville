using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    private static List<Food> foodList = new List<Food>();

    private void Awake()
    {
        foodList.Add(this);
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
