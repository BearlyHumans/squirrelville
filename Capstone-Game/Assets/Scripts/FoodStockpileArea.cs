using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class FoodStockpileArea : MonoBehaviour
{
    private MeshRenderer mesh;
    private GameObject[] food;

    private void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        food = GetAllFood();
    }

    public GameObject[] GetFoodInArea()
    {
        List<GameObject> foodInArea = new List<GameObject>();

        foreach (GameObject obj in food)
        {
            if (obj.activeInHierarchy && mesh.bounds.Contains(obj.transform.position))
            {
                foodInArea.Add(obj);
            }
        }

        return foodInArea.ToArray();
    }

    public int GetFoodCount()
    {
        return GetFoodInArea().Length;
    }

    private GameObject[] GetAllFood()
    {
        GameObject[] objects = FindObjectsOfType<GameObject>();
        List<GameObject> objList = new List<GameObject>();
        int foodLayer = LayerMask.NameToLayer("Food");

        foreach (GameObject obj in objects)
        {
            if (obj.layer == foodLayer)
            {
                objList.Add(obj);
            }
        }

        return objList.ToArray();
    }
}
