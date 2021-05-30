using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Collider))]
public class FoodStockpileArea : MonoBehaviour
{
    private MeshRenderer mesh;
    private Collider trigger;
    private GameObject[] food;
    public TMP_Text label;
    private GameObject[] oldFoodInArea;

    private void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        trigger = GetComponent<Collider>();
        food = GetAllFood();
        mesh.enabled = false;
    }

    private void Update()
    {
        label.text = $"Food collected: {GetFoodCount()}";

        if (oldFoodInArea != null)
        {
            foreach (GameObject obj in oldFoodInArea)
            {
                if (!obj.activeInHierarchy)
                {
                    Debug.Log($"{obj.name} was eaten");
                }
            }
        }

        oldFoodInArea = GetFoodInArea();
    }

    private void OnTriggerEnter(Collider other)
    {
        int foodLayer = LayerMask.NameToLayer("Food");
        if (other.gameObject.layer == foodLayer)
        {
            Debug.Log($"{other.name} entered");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        int foodLayer = LayerMask.NameToLayer("Food");
        if (other.gameObject.layer == foodLayer)
        {
            Debug.Log($"{other.name} rolled out");
        }
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
